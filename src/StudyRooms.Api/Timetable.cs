using System.Diagnostics.CodeAnalysis;
using StudyRooms.Api.Domain;

namespace StudyRooms.Api;

// Holds every room and booking, and checks the rules that need to see all of them.
// There is one Timetable for the whole application (a singleton), so every request sees the same bookings.
public class Timetable
{
    private readonly List<Room> _rooms = [];
    private readonly List<Booking> _bookings = [];
    private readonly List<User> _users = [];
    private int _nextRoomId = 1;
    private int _nextBookingId = 1;

    public Room AddRoom(string name, string building, int capacity, Equipment equipment)
    {
        Room room = new Room(_nextRoomId++, name, building, capacity, equipment);
        _rooms.Add(room);
        return room;
    }

    public void AddUser(string id, string displayName, string email)
    {
        _users.Add(new User(id, displayName, email));
    }

    public User? FindUser(string id)
    {
        foreach (User user in _users)
        {
            if (user.Id == id)
            {
                return user;
            }
        }
        return null;
    }

    // For ids that must exist, such as the owner of a booking.
    public User GetUser(string id)
    {
        return FindUser(id) ?? throw new InvalidOperationException($"User {id} does not exist.");
    }

    public List<Room> GetRooms()
    {
        return _rooms;
    }

    public Room? FindRoom(int id)
    {
        foreach (Room room in _rooms)
        {
            if (room.Id == id)
            {
                return room;
            }
        }
        return null;
    }

    // For ids that must exist, such as the room of a booking.
    public Room GetRoom(int id)
    {
        return FindRoom(id) ?? throw new InvalidOperationException($"Room {id} does not exist.");
    }

    public Booking? FindBooking(int id)
    {
        foreach (Booking booking in _bookings)
        {
            if (booking.Id == id)
            {
                return booking;
            }
        }
        return null;
    }

    public List<Booking> GetBookings(DateOnly date)
    {
        List<Booking> result = [];
        foreach (Booking booking in _bookings)
        {
            if (booking.Date == date && booking.Status == BookingStatus.Active)
            {
                result.Add(booking);
            }
        }
        return result;
    }

    public bool TryBook(int roomId, DateOnly date, TimeOnly start, TimeOnly end, string userId, JoinPolicy joinPolicy,
        [NotNullWhen(true)] out Booking? booking, [NotNullWhen(false)] out string? error)
    {
        foreach (Booking other in _bookings)
        {
            if (other.Status != BookingStatus.Active || !other.Overlaps(date, start, end))
            {
                continue;
            }

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

        booking = new Booking(_nextBookingId++, roomId, userId, date, start, end, joinPolicy);
        _bookings.Add(booking);
        error = null;
        return true;
    }

    public bool TryJoin(Booking booking, string userId, DateTime now, [NotNullWhen(false)] out string? error)
    {
        // Joining must not put the student in two places at once either.
        foreach (Booking other in _bookings)
        {
            if (other != booking && other.Status == BookingStatus.Active
                && other.Overlaps(booking.Date, booking.Start, booking.End) && other.HasJoined(userId))
            {
                error = $"{userId} is already in booking {other.Id}, from {other.Start} to {other.End}.";
                return false;
            }
        }

        Room room = GetRoom(booking.RoomId);
        return booking.TryJoin(userId, room.Capacity, now, out error);
    }
}
