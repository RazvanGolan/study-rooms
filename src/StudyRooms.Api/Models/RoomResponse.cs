using StudyRooms.Api.Domain;

namespace StudyRooms.Api.Models;

public record RoomResponse(int Id, string Name, string Building, int Capacity, Equipment Equipment)
{
    public static RoomResponse From(Room room)
    {
        return new RoomResponse(room.Id, room.Name, room.Building, room.Capacity, room.Equipment);
    }
}
