using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Domain;
using StudyRooms.Api.Models;
using StudyRooms.Api.Services;

namespace StudyRooms.Api.Controllers;

// Only admins. A student with a valid token gets 403, someone without a token gets 401.
[ApiController]
[Route("admin/rooms")]
[Authorize(Roles = Roles.Admin)]
public class AdminRoomsController : AppControllerBase
{
    private readonly RoomService _roomService;
    private readonly BookingRules _rules;
    private readonly ILogger<AdminRoomsController> _logger;

    public AdminRoomsController(RoomService roomService, BookingRules rules, ILogger<AdminRoomsController> logger)
    {
        _roomService = roomService;
        _rules = rules;
        _logger = logger;
    }

    // POST /admin/rooms
    [HttpPost]
    public IActionResult Add(RoomRequest request)
    {
        List<string> problems = CheckRoom(request);
        if (problems.Count > 0)
        {
            return BadRequest(new ErrorResponse("The room is not valid.", problems));
        }

        if (!_roomService.TryAdd(request.Name, request.Building, request.Capacity, request.Equipment,
                out Room? room, out string? error))
        {
            return Conflict(new ErrorResponse("The room was not added.", [error]));
        }
        return Created($"/rooms/{room.Id}", RoomResponse.From(room));
    }

    // PUT /admin/rooms/1: replaces every detail of the room
    [HttpPut("{id}")]
    public IActionResult Edit(int id, RoomRequest request)
    {
        Room? room = _roomService.FindRoom(id);
        if (room is null)
        {
            return NotFound();
        }

        List<string> problems = CheckRoom(request);
        if (problems.Count > 0)
        {
            return BadRequest(new ErrorResponse("The room is not valid.", problems));
        }

        if (!_roomService.TryEdit(room, request.Name, request.Building, request.Capacity, request.Equipment,
                out string? error))
        {
            return Conflict(new ErrorResponse("The room was not changed.", [error]));
        }
        return Ok(RoomResponse.From(room));
    }

    // DELETE /admin/rooms/1: hides the room and cancels its future bookings
    [HttpDelete("{id}")]
    public IActionResult Remove(int id)
    {
        Room? room = _roomService.FindRoom(id);
        if (room is null)
        {
            return NotFound();
        }

        List<Booking> cancelled = _roomService.Remove(room, _rules.Now());
        foreach (Booking booking in cancelled)
        {
            _logger.LogInformation("Booking {BookingId} cancelled, room {RoomId} was removed; people to notify: {Count}",
                booking.Id, room.Id, booking.CountJoined());
        }
        return NoContent();
    }

    private static List<string> CheckRoom(RoomRequest request)
    {
        List<string> problems = [];
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            problems.Add("The room needs a name.");
        }
        if (string.IsNullOrWhiteSpace(request.Building))
        {
            problems.Add("The room needs a building.");
        }
        if (request.Capacity < 1)
        {
            problems.Add("The capacity must be at least 1.");
        }
        return problems;
    }
}
