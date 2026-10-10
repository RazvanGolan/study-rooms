using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Domain;
using StudyRooms.Api.Models;

namespace StudyRooms.Api.Controllers;

[ApiController]
[Route("rooms")]
public class RoomsController : ControllerBase
{
    private readonly Timetable _timetable;

    public RoomsController(Timetable timetable)
    {
        _timetable = timetable;
    }

    // GET /rooms
    [HttpGet]
    public List<RoomResponse> GetAll()
    {
        List<RoomResponse> rooms = [];
        foreach (Room room in _timetable.GetRooms())
        {
            rooms.Add(RoomResponse.From(room));
        }
        return rooms;
    }

    // GET /rooms/2
    [HttpGet("{id}")]
    public ActionResult<RoomResponse> Get(int id)
    {
        Room? room = _timetable.FindRoom(id);
        if (room is null)
        {
            return NotFound();
        }
        return RoomResponse.From(room);
    }
}
