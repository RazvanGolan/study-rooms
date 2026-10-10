using System.Diagnostics.CodeAnalysis;
using StudyRooms.Api.Domain;

namespace StudyRooms.Api;

// Holds every room and booking, and checks the rules that need to see all of them.
// There is one Timetable for the whole application (a singleton), so every request sees the same bookings.
public class Timetable
{
    private readonly List<Room> _rooms = [];
    private readonly List<Booking> _bookings = [];
    private int _nextRoomId = 1;
    private int _nextBookingId = 1;

    public Room AddRoom(string name, string building, int capacity, Equipment equipment)
    {
        Room room = new Room(_nextRoomId++, name, building, capacity, equipment);
        _rooms.Add(room);
        return room;
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
}
