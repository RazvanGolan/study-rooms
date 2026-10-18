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
    private readonly ILogger<BookingService> _logger;

    public BookingService(AppDbContext db, IOptions<BookingOptions> options, ILogger<BookingService> logger)
    {
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    // Rooms an admin has removed cannot be booked.
    public Room? FindActiveRoom(int id)
    {
        Room? room = _db.Rooms.Find(id);
        if (room is null || !room.IsActive)
        {
            return null;
        }
        return room;
    }

    public Booking? FindBooking(int id)
    {
        return BookingsWithDetails().FirstOrDefault(b => b.Id == id);
    }

    // The timetable: active bookings and blocks on one day, in every room.
    public List<Booking> GetBookings(DateOnly date)
    {
        return BookingsWithDetails()
            .Where(b => b.Date == date && b.Status == BookingStatus.Active)
            .OrderBy(b => b.Start)
            .ToList();
    }

    // For admins: every booking on one day, the cancelled ones too.
    public List<Booking> GetAllBookings(DateOnly date)
    {
        return BookingsWithDetails()
            .Where(b => b.Date == date)
            .OrderBy(b => b.Start)
            .ToList();
    }

    // The bookings a student is in, or has asked to join.
    public List<Booking> GetBookingsOf(string userId)
    {
        List<int> bookingIds = _db.Participants
            .Where(p => p.UserId == userId
                     && (p.Status == ParticipantStatus.Joined || p.Status == ParticipantStatus.Requested))
            .Select(p => p.BookingId)
            .ToList();

        return BookingsWithDetails()
            .Where(b => bookingIds.Contains(b.Id))
            .OrderBy(b => b.Date)
            .ThenBy(b => b.Start)
            .ToList();
    }

    public bool TryBook(Room room, AppUser owner, DateOnly date, TimeOnly start, TimeOnly end, JoinPolicy joinPolicy,
        [NotNullWhen(true)] out Booking? booking, [NotNullWhen(false)] out string? error)
    {
        // The check and the insert happen in one transaction. Until it ends, no other request can write,
        // so nobody can book the room between our check and our insert.
        using IDbContextTransaction transaction = _db.Database.BeginTransaction();

        foreach (Booking other in FindOverlapping(date, start, end))
        {
            // The room is already booked for part of this time.
            if (other.RoomId == room.Id)
            {
                booking = null;
                error = $"The room is taken from {other.Start} to {other.End} (booking {other.Id}).";
                return false;
            }

            // The student is already in another booking at this time, in any room.
            if (other.HasJoined(owner.Id))
            {
                booking = null;
                error = $"{owner.DisplayName} is already in booking {other.Id}, from {other.Start} to {other.End}.";
                return false;
            }
        }

        WaitForDemo();

        booking = new Booking(room, owner, date, start, end, joinPolicy);
        _db.Bookings.Add(booking);
        _db.SaveChanges();
        transaction.Commit();
        error = null;
        return true;
    }

    // A block only needs the room to be free. It has nobody in it.
    public bool TryBlock(Room room, DateOnly date, TimeOnly start, TimeOnly end, string note,
        [NotNullWhen(true)] out Booking? booking, [NotNullWhen(false)] out string? error)
    {
        using IDbContextTransaction transaction = _db.Database.BeginTransaction();

        foreach (Booking other in FindOverlapping(date, start, end))
        {
            if (other.RoomId == room.Id)
            {
                booking = null;
                error = $"The room is taken from {other.Start} to {other.End} (booking {other.Id}). Cancel that booking first.";
                return false;
            }
        }

        booking = new Booking(room, date, start, end, note);
        _db.Bookings.Add(booking);
        _db.SaveChanges();
        transaction.Commit();
        error = null;
        return true;
    }

    public bool TryJoin(int bookingId, AppUser user, DateTime now,
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
        if (!IsFreeAt(user.Id, booking, out error))
        {
            return false;
        }

        if (!booking.TryJoin(user, now, out error))
        {
            return false;
        }

        WaitForDemo();

        if (booking.IsFull())
        {
            DeclinePendingRequests(booking);
        }

        _db.SaveChanges();
        transaction.Commit();
        return true;
    }

    // Only the owner approves. When someone else asks, booking is set and error says why,
    // and the controller answers 403.
    public bool TryApprove(int bookingId, string ownerId, string requesterId, DateTime now,
        [NotNullWhen(true)] out Booking? booking, [NotNullWhen(false)] out string? error)
    {
        // Approving takes a place, like joining, so it reads, checks and saves in one transaction.
        using IDbContextTransaction transaction = _db.Database.BeginTransaction();

        booking = FindBooking(bookingId);
        if (booking is null)
        {
            error = $"There is no booking {bookingId}.";
            return false;
        }
        if (booking.OwnerId != ownerId)
        {
            error = "Only the owner can approve requests.";
            return false;
        }

        // Pending requests do not count as being somewhere, so the student may be busy by now.
        if (!IsFreeAt(requesterId, booking, out error))
        {
            return false;
        }

        if (!booking.TryApprove(requesterId, now, out error))
        {
            return false;
        }

        // The student is busy at this time now, so their other requests for the same time are declined.
        foreach (Booking other in FindOverlapping(booking.Date, booking.Start, booking.End))
        {
            if (other.Id != booking.Id && other.HasRequested(requesterId))
            {
                other.FindParticipant(requesterId)!.MarkDeclined();
                _logger.LogInformation("Request of {UserId} to join booking {BookingId} declined, busy at that time; notify them",
                    requesterId, other.Id);
            }
        }

        if (booking.IsFull())
        {
            DeclinePendingRequests(booking);
        }

        _db.SaveChanges();
        transaction.Commit();
        return true;
    }

    public bool TryDecline(Booking booking, string requesterId, DateTime now, [NotNullWhen(false)] out string? error)
    {
        if (!booking.TryDecline(requesterId, now, out error))
        {
            return false;
        }
        _db.SaveChanges();
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

    public bool TryCancelAsAdmin(Booking booking, [NotNullWhen(false)] out string? error)
    {
        if (!booking.TryCancelAsAdmin(out error))
        {
            return false;
        }
        _db.SaveChanges();
        return true;
    }

    // Every query that returns bookings to show loads the same related objects:
    // the room, the owner, and each participant with their account, in the order they joined.
    private IQueryable<Booking> BookingsWithDetails()
    {
        return _db.Bookings
            .Include(b => b.Room)
            .Include(b => b.Owner)
            .Include(b => b.Participants.OrderBy(p => p.CreatedAt))
            .ThenInclude(p => p.User);
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

    // A student cannot be in two places at once: no other active booking they are in may overlap this one.
    private bool IsFreeAt(string userId, Booking booking, [NotNullWhen(false)] out string? error)
    {
        foreach (Booking other in FindOverlapping(booking.Date, booking.Start, booking.End))
        {
            if (other.Id != booking.Id && other.HasJoined(userId))
            {
                error = $"The student is already in booking {other.Id}, from {other.Start} to {other.End}.";
                return false;
            }
        }
        error = null;
        return true;
    }

    private void DeclinePendingRequests(Booking booking)
    {
        foreach (Participant declined in booking.DeclinePendingRequests())
        {
            _logger.LogInformation("Request of {UserId} to join booking {BookingId} declined, the booking is full; notify them",
                declined.UserId, booking.Id);
        }
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
