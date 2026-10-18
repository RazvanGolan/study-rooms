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
    public List<Room> GetRooms(Equipment equipment)
    {
        return _db.Rooms
            .Where(r => (r.Equipment & equipment) == equipment)
            .OrderBy(r => r.Name)
            .ToList();
    }

    public Room? FindRoom(int id)
    {
        return _db.Rooms.Find(id);
    }
}
