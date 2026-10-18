using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Domain;
using StudyRooms.Api.Models;
using StudyRooms.Api.Services;

namespace StudyRooms.Api.Controllers;

[ApiController]
[Route("admin/bookings")]
[Authorize(Roles = Roles.Admin)]
public class AdminBookingsController : AppControllerBase
{
    private readonly BookingService _bookingService;
    private readonly AccountService _accountService;
    private readonly BookingRules _rules;
    private readonly ILogger<AdminBookingsController> _logger;

    public AdminBookingsController(BookingService bookingService, AccountService accountService, BookingRules rules,
        ILogger<AdminBookingsController> logger)
    {
        _bookingService = bookingService;
        _accountService = accountService;
        _rules = rules;
        _logger = logger;
    }

    // GET /admin/bookings?date=2026-10-27: every booking that day, cancelled ones too, with emails
    [HttpGet]
    public List<AdminBookingResponse> GetAll(DateOnly date)
    {
        List<AdminBookingResponse> bookings = [];
        foreach (Booking booking in _bookingService.GetAllBookings(date))
        {
            bookings.Add(AdminBookingResponse.From(booking));
        }
        return bookings;
    }

    // POST /admin/bookings: a booking for a student. An admin may book further ahead than 7 days,
    // but the room and the student must still be free.
    [HttpPost]
    public IActionResult Create(AdminBookingRequest request)
    {
        Room? room = _bookingService.FindActiveRoom(request.RoomId);
        if (room is null)
        {
            return NotFound(new ErrorResponse("The room does not exist.", [$"There is no room {request.RoomId}."]));
        }

        List<string> problems = _rules.CheckTimesForAdmin(request.Date, request.Start, request.End);
        AppUser? student = _accountService.FindByEmail(request.StudentEmail);
        if (student is null)
        {
            problems.Add($"There is no student with the email {request.StudentEmail}.");
        }
        if (problems.Count > 0 || student is null)
        {
            return BadRequest(new ErrorResponse("The booking is not valid.", problems));
        }

        if (!_bookingService.TryBook(room, student, CurrentUserId, request.Date, request.Start, request.End, request.JoinPolicy,
                out Booking? booking, out string? error))
        {
            return Conflict(new ErrorResponse("The booking was refused.", [error]));
        }

        _logger.LogInformation("Booking {BookingId} created by an admin for {UserId}; notify them", booking.Id, student.Id);
        return Created($"/bookings/{booking.Id}", AdminBookingResponse.From(booking));
    }

    // POST /admin/bookings/blocks: the room is taken for a class or for maintenance
    [HttpPost("blocks")]
    public IActionResult CreateBlock(BlockRequest request)
    {
        Room? room = _bookingService.FindActiveRoom(request.RoomId);
        if (room is null)
        {
            return NotFound(new ErrorResponse("The room does not exist.", [$"There is no room {request.RoomId}."]));
        }

        List<string> problems = _rules.CheckTimesForAdmin(request.Date, request.Start, request.End);
        if (string.IsNullOrWhiteSpace(request.Note))
        {
            problems.Add("A block needs a note, such as \"Class: PCLP\".");
        }
        if (problems.Count > 0)
        {
            return BadRequest(new ErrorResponse("The block is not valid.", problems));
        }

        if (!_bookingService.TryBlock(room, request.Date, request.Start, request.End, request.Note,
                out Booking? booking, out string? error))
        {
            return Conflict(new ErrorResponse("The block was refused.", [error]));
        }
        return Created($"/bookings/{booking.Id}", AdminBookingResponse.From(booking));
    }

    // POST /admin/bookings/1/cancel: any booking, even after the owner's cutoff
    [HttpPost("{id}/cancel")]
    public IActionResult Cancel(int id)
    {
        Booking? booking = _bookingService.FindBooking(id);
        if (booking is null)
        {
            return NotFound();
        }

        if (!_bookingService.TryCancelAsAdmin(booking, out string? error))
        {
            return Conflict(new ErrorResponse("The booking cannot be cancelled.", [error]));
        }

        _logger.LogInformation("Booking {BookingId} cancelled by an admin; people to notify: {Count}",
            booking.Id, booking.CountJoined());
        return Ok(AdminBookingResponse.From(booking));
    }
}
