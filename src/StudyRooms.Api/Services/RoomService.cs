using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using StudyRooms.Api.Data;
using StudyRooms.Api.Domain;

namespace StudyRooms.Api.Services;

public class RoomService
{
    private readonly AppDbContext _db;

    public RoomService(AppDbContext db)
    {
        _db = db;
    }

    // The rooms that have all the equipment asked for. Equipment.None asks for nothing, so it returns every room.
    // Rooms an admin has removed are hidden.
    public List<Room> GetRooms(Equipment equipment)
    {
        return _db.Rooms
            .Where(r => r.IsActive && (r.Equipment & equipment) == equipment)
            .OrderBy(r => r.Name)
            .ToList();
    }

    public Room? FindRoom(int id)
    {
        Room? room = _db.Rooms.Find(id);
        if (room is null || !room.IsActive)
        {
            return null;
        }
        return room;
    }

    public bool TryAdd(string name, string building, int capacity, Equipment equipment,
        [NotNullWhen(true)] out Room? room, [NotNullWhen(false)] out string? error)
    {
        room = new Room(name, building, capacity, equipment);
        _db.Rooms.Add(room);
        return TrySave(name, out error);
    }

    public bool TryEdit(Room room, string name, string building, int capacity, Equipment equipment,
        [NotNullWhen(false)] out string? error)
    {
        room.Edit(name, building, capacity, equipment);
        return TrySave(name, out error);
    }

    // Hides the room and cancels its bookings that have not started yet.
    // Returns those bookings, so that everyone in them can be told.
    public List<Booking> Remove(Room room, DateTime now)
    {
        DateOnly today = DateOnly.FromDateTime(now);
        TimeOnly timeNow = TimeOnly.FromDateTime(now);

        List<Booking> cancelled = _db.Bookings
            .Include(b => b.Room)
            .Include(b => b.Participants)
            .Where(b => b.RoomId == room.Id && b.Status == BookingStatus.Active
                     && (b.Date > today || (b.Date == today && b.Start > timeNow)))
            .ToList();

        room.Remove();
        foreach (Booking booking in cancelled)
        {
            booking.CancelBecauseRoomRemoved();
        }

        _db.SaveChanges();
        return cancelled;
    }

    // Room names are unique. The database checks that with the unique index on Name,
    // and refuses the save with an exception when a name is taken.
    private bool TrySave(string name, [NotNullWhen(false)] out string? error)
    {
        try
        {
            _db.SaveChanges();
        }
        catch (DbUpdateException)
        {
            error = $"There is already a room called {name}.";
            return false;
        }

        error = null;
        return true;
    }
}
