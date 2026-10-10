using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Domain;

namespace StudyRooms.Api.Controllers;

[ApiController]
[Route("bookings")]
public class BookingsController : ControllerBase
{
    private readonly Timetable _timetable;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(Timetable timetable, ILogger<BookingsController> logger)
    {
        _timetable = timetable;
        _logger = logger;
    }

    // GET /bookings/1
    [HttpGet("{id}")]
    public ActionResult<Booking> Get(int id)
    {
        Booking? booking = _timetable.FindBooking(id);
        if (booking is null)
        {
            return NotFound();
        }
        return booking;
    }

    // POST /bookings, with a BookingRequest as JSON in the body
    [HttpPost]
    public IActionResult Create(BookingRequest request)
    {
        if (_timetable.FindRoom(request.RoomId) is null)
        {
            return NotFound($"Room {request.RoomId} does not exist.");
        }
        if (request.End <= request.Start)
        {
            return BadRequest("A booking must end after it starts.");
        }

        if (!_timetable.TryBook(request.RoomId, request.Date, request.Start, request.End, request.UserId,
                request.JoinPolicy, out Booking? booking, out string? error))
        {
            _logger.LogInformation("Booking refused for {UserId}: {Reason}", request.UserId, error);
            return Conflict(error);
        }

        _logger.LogInformation("Booking {BookingId} created by {UserId} for room {RoomId}, {Date} {Start}-{End}",
            booking.Id, booking.OwnerId, booking.RoomId, booking.Date, booking.Start, booking.End);
        return Created($"/bookings/{booking.Id}", booking);
    }
}
