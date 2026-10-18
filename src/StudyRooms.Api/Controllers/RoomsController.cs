using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Domain;
using StudyRooms.Api.Models;
using StudyRooms.Api.Services;

namespace StudyRooms.Api.Controllers;

[ApiController]
[Route("rooms")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly RoomService _roomService;

    public RoomsController(RoomService roomService)
    {
        _roomService = roomService;
    }

    // GET /rooms, or GET /rooms?equipment=Projector,PowerOutlets for the rooms that have both
    [HttpGet]
    public List<RoomResponse> GetAll(Equipment equipment = Equipment.None)
    {
        List<RoomResponse> rooms = [];
        foreach (Room room in _roomService.GetRooms(equipment))
        {
            rooms.Add(RoomResponse.From(room));
        }
        return rooms;
    }

    // GET /rooms/2
    [HttpGet("{id}")]
    public ActionResult<RoomResponse> Get(int id)
    {
        Room? room = _roomService.FindRoom(id);
        if (room is null)
        {
            return NotFound();
        }
        return RoomResponse.From(room);
    }
}
