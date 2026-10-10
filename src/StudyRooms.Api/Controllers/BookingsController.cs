using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Domain;
using StudyRooms.Api.Models;

namespace StudyRooms.Api.Controllers;

[ApiController]
[Route("bookings")]
public class BookingsController : ControllerBase
{
    private readonly Timetable _timetable;
    private readonly BookingRules _rules;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(Timetable timetable, BookingRules rules, ILogger<BookingsController> logger)
    {
        _timetable = timetable;
        _rules = rules;
        _logger = logger;
    }

    // GET /bookings/1
    [HttpGet("{id}")]
    public ActionResult<BookingResponse> Get(int id)
    {
        Booking? booking = _timetable.FindBooking(id);
        if (booking is null)
        {
            return NotFound();
        }
        return BookingResponse.From(booking, _timetable);
    }

    // POST /bookings
    [HttpPost]
    public IActionResult Create(BookingRequest request)
    {
        if (_timetable.FindRoom(request.RoomId) is null)
        {
            return NotFound(new ErrorResponse("The room does not exist.", [$"There is no room {request.RoomId}."]));
        }

        List<string> problems = _rules.CheckTimes(request.Date, request.Start, request.End);
        if (_timetable.FindUser(request.UserId) is null)
        {
            problems.Add($"There is no user {request.UserId}.");
        }
        if (problems.Count > 0)
        {
            return BadRequest(new ErrorResponse("The booking is not valid.", problems));
        }

        if (!_timetable.TryBook(request.RoomId, request.Date, request.Start, request.End, request.UserId,
                request.JoinPolicy, out Booking? booking, out string? error))
        {
            _logger.LogInformation("Booking refused for {UserId}: {Reason}", request.UserId, error);
            return Conflict(new ErrorResponse("The booking was refused.", [error]));
        }

        _logger.LogInformation("Booking {BookingId} created by {UserId} for room {RoomId}, {Date} {Start}-{End}",
            booking.Id, booking.OwnerId, booking.RoomId, booking.Date, booking.Start, booking.End);
        return Created($"/bookings/{booking.Id}", BookingResponse.From(booking, _timetable));
    }

    // POST /bookings/1/join
    [HttpPost("{id}/join")]
    public IActionResult Join(int id, UserRequest request)
    {
        Booking? booking = _timetable.FindBooking(id);
        if (booking is null)
        {
            return NotFound();
        }
        if (_timetable.FindUser(request.UserId) is null)
        {
            return NotFound(new ErrorResponse("The user does not exist.", [$"There is no user {request.UserId}."]));
        }

        if (!_timetable.TryJoin(booking, request.UserId, _rules.Now(), out string? error))
        {
            return Conflict(new ErrorResponse("You cannot join this booking.", [error]));
        }

        _logger.LogInformation("{UserId} joined booking {BookingId}; notify the owner", request.UserId, booking.Id);
        return Ok(BookingResponse.From(booking, _timetable));
    }

    // POST /bookings/1/leave
    [HttpPost("{id}/leave")]
    public IActionResult Leave(int id, UserRequest request)
    {
        Booking? booking = _timetable.FindBooking(id);
        if (booking is null)
        {
            return NotFound();
        }

        if (!booking.TryLeave(request.UserId, _rules.Now(), out string? error))
        {
            return Conflict(new ErrorResponse("You cannot leave this booking.", [error]));
        }

        _logger.LogInformation("{UserId} left booking {BookingId}; notify the owner", request.UserId, booking.Id);
        return Ok(BookingResponse.From(booking, _timetable));
    }

    // POST /bookings/1/participants/mihai/remove, sent by the owner
    [HttpPost("{id}/participants/{participantId}/remove")]
    public IActionResult Remove(int id, string participantId, UserRequest request)
    {
        Booking? booking = _timetable.FindBooking(id);
        if (booking is null)
        {
            return NotFound();
        }
        if (request.UserId != booking.OwnerId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ErrorResponse("You cannot remove people.", ["Only the owner can remove people."]));
        }

        if (!booking.TryRemove(participantId, _rules.Now(), out string? error))
        {
            return Conflict(new ErrorResponse("This person cannot be removed.", [error]));
        }

        _logger.LogInformation("{ParticipantId} removed from booking {BookingId}; notify them", participantId, booking.Id);
        return Ok(BookingResponse.From(booking, _timetable));
    }

    // POST /bookings/1/cancel, sent by the owner
    [HttpPost("{id}/cancel")]
    public IActionResult Cancel(int id, UserRequest request)
    {
        Booking? booking = _timetable.FindBooking(id);
        if (booking is null)
        {
            return NotFound();
        }
        if (request.UserId != booking.OwnerId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ErrorResponse("You cannot cancel this booking.", ["Only the owner can cancel the booking."]));
        }

        if (!booking.TryCancel(_rules.Now(), _rules.CancelCutoff, out string? error))
        {
            return Conflict(new ErrorResponse("The booking cannot be cancelled.", [error]));
        }

        _logger.LogInformation("Booking {BookingId} cancelled by its owner; people to notify: {Count}",
            booking.Id, booking.CountJoined());
        return Ok(BookingResponse.From(booking, _timetable));
    }
}
