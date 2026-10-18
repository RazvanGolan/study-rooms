using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using StudyRooms.Api.Data;
using StudyRooms.Api.Domain;

namespace StudyRooms.Api.Services;

// The rules that need to see more than one booking, and saving the changes.
// It uses the AppDbContext of the current request, so it is scoped too: one BookingService per request.
public class BookingService
{
    private readonly AppDbContext _db;
    private readonly BookingOptions _options;

    public BookingService(AppDbContext db, IOptions<BookingOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public User? FindUser(string id)
    {
        return _db.Users.Find(id);
    }

    // For ids that must exist, such as the owner of a booking.
    public User GetUser(string id)
    {
        return FindUser(id) ?? throw new InvalidOperationException($"User {id} does not exist.");
    }

    public Room? FindRoom(int id)
    {
        return _db.Rooms.Find(id);
    }

    // For ids that must exist, such as the room of a booking.
    public Room GetRoom(int id)
    {
        return FindRoom(id) ?? throw new InvalidOperationException($"Room {id} does not exist.");
    }

    public Booking? FindBooking(int id)
    {
        return _db.Bookings
            .Include(b => b.Participants)
            .FirstOrDefault(b => b.Id == id);
    }

    public List<Booking> GetBookings(DateOnly date)
    {
        return _db.Bookings
            .Include(b => b.Participants)
            .Where(b => b.Date == date && b.Status == BookingStatus.Active)
            .OrderBy(b => b.Start)
            .ToList();
    }

    public bool TryBook(int roomId, DateOnly date, TimeOnly start, TimeOnly end, string userId, JoinPolicy joinPolicy,
        [NotNullWhen(true)] out Booking? booking, [NotNullWhen(false)] out string? error)
    {
        // The check and the insert happen in one transaction. Until it ends, no other request can write,
        // so nobody can book the room between our check and our insert.
        using IDbContextTransaction transaction = _db.Database.BeginTransaction();

        foreach (Booking other in FindOverlapping(date, start, end))
        {
            // The room is already booked for part of this time.
            if (other.RoomId == roomId)
            {
                booking = null;
                error = $"The room is taken from {other.Start} to {other.End} (booking {other.Id}).";
                return false;
            }

            // The student is already in another booking at this time, in any room.
            if (other.HasJoined(userId))
            {
                booking = null;
                error = $"{userId} is already in booking {other.Id}, from {other.Start} to {other.End}.";
                return false;
            }
        }

        WaitForDemo();

        booking = new Booking(roomId, userId, date, start, end, joinPolicy);
        _db.Bookings.Add(booking);
        _db.SaveChanges();
        transaction.Commit();
        error = null;
        return true;
    }

    public bool TryJoin(int bookingId, string userId, DateTime now,
        [NotNullWhen(true)] out Booking? booking, [NotNullWhen(false)] out string? error)
    {
        // Two students could take the last place at the same time, so the booking is read,
        // checked and saved in one transaction too.
        using IDbContextTransaction transaction = _db.Database.BeginTransaction();

        booking = FindBooking(bookingId);
        if (booking is null)
        {
            error = $"There is no booking {bookingId}.";
            return false;
        }

        // Joining must not put the student in two places at once either.
        foreach (Booking other in FindOverlapping(booking.Date, booking.Start, booking.End))
        {
            if (other.Id != booking.Id && other.HasJoined(userId))
            {
                error = $"{userId} is already in booking {other.Id}, from {other.Start} to {other.End}.";
                return false;
            }
        }

        Room room = GetRoom(booking.RoomId);
        if (!booking.TryJoin(userId, room.Capacity, now, out error))
        {
            return false;
        }

        WaitForDemo();

        _db.SaveChanges();
        transaction.Commit();
        return true;
    }

    public bool TryLeave(Booking booking, string userId, DateTime now, [NotNullWhen(false)] out string? error)
    {
        if (!booking.TryLeave(userId, now, out error))
        {
            return false;
        }
        _db.SaveChanges();
        return true;
    }

    public bool TryRemove(Booking booking, string participantId, DateTime now, [NotNullWhen(false)] out string? error)
    {
        if (!booking.TryRemove(participantId, now, out error))
        {
            return false;
        }
        _db.SaveChanges();
        return true;
    }

    public bool TryCancel(Booking booking, DateTime now, TimeSpan cutoff, [NotNullWhen(false)] out string? error)
    {
        if (!booking.TryCancel(now, cutoff, out error))
        {
            return false;
        }
        _db.SaveChanges();
        return true;
    }

    // Active bookings on the same day that overlap the given times, in any room.
    // Two time ranges overlap when each one starts before the other ends.
    // 10:00-11:00 and 11:00-12:00 only touch, so they do not overlap.
    // EF Core turns the lambda into SQL, so it can only use what SQL can do: comparisons, and, or.
    private List<Booking> FindOverlapping(DateOnly date, TimeOnly start, TimeOnly end)
    {
        return _db.Bookings
            .Include(b => b.Participants)
            .Where(b => b.Date == date && b.Status == BookingStatus.Active && b.Start < end && start < b.End)
            .ToList();
    }

    // For the demo in class: a pause between the check and the save makes two requests
    // sent at the same moment collide every time. It is 0 (no pause) unless appsettings.json says otherwise.
    private void WaitForDemo()
    {
        if (_options.DemoDelaySeconds > 0)
        {
            Thread.Sleep(TimeSpan.FromSeconds(_options.DemoDelaySeconds));
        }
    }
}
