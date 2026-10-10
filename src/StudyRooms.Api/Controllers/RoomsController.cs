using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Domain;

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
    public List<Room> GetAll()
    {
        return _timetable.GetRooms();
    }

    // GET /rooms/2
    [HttpGet("{id}")]
    public ActionResult<Room> Get(int id)
    {
        Room? room = _timetable.FindRoom(id);
        if (room is null)
        {
            return NotFound();
        }
        return room;
    }
}
