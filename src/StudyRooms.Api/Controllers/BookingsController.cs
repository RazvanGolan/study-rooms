using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Domain;
using StudyRooms.Api.Models;
using StudyRooms.Api.Services;

namespace StudyRooms.Api.Controllers;

// [Authorize]: every endpoint here needs a logged-in user. Without a token the answer is 401.
[ApiController]
[Route("bookings")]
[Authorize]
public class BookingsController : AppControllerBase
{
    private readonly BookingService _bookingService;
    private readonly AccountService _accountService;
    private readonly BookingRules _rules;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(BookingService bookingService, AccountService accountService, BookingRules rules,
        ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _accountService = accountService;
        _rules = rules;
        _logger = logger;
    }

    // GET /bookings/1
    [HttpGet("{id}")]
    public ActionResult<BookingResponse> Get(int id)
    {
        Booking? booking = _bookingService.FindBooking(id);
        if (booking is null)
        {
            return NotFound();
        }
        return BookingResponse.From(booking, CurrentUserId);
    }

    // POST /bookings
    [HttpPost]
    public IActionResult Create(BookingRequest request)
    {
        Room? room = _bookingService.FindActiveRoom(request.RoomId);
        if (room is null)
        {
            return NotFound(new ErrorResponse("The room does not exist.", [$"There is no room {request.RoomId}."]));
        }

        List<string> problems = _rules.CheckTimes(request.Date, request.Start, request.End);
        if (problems.Count > 0)
        {
            return BadRequest(new ErrorResponse("The booking is not valid.", problems));
        }

        AppUser owner = _accountService.FindUser(CurrentUserId)!;
        if (!_bookingService.TryBook(room, owner, request.Date, request.Start, request.End, request.JoinPolicy,
                out Booking? booking, out string? error))
        {
            _logger.LogInformation("Booking refused for {UserId}: {Reason}", owner.Id, error);
            return Conflict(new ErrorResponse("The booking was refused.", [error]));
        }

        _logger.LogInformation("Booking {BookingId} created by {UserId} for room {RoomId}, {Date} {Start}-{End}",
            booking.Id, booking.OwnerId, booking.RoomId, booking.Date, booking.Start, booking.End);
        return Created($"/bookings/{booking.Id}", BookingResponse.From(booking, CurrentUserId));
    }

    // POST /bookings/1/join
    // Joins an open booking, or asks to join one with the "on request" policy.
    [HttpPost("{id}/join")]
    public IActionResult Join(int id)
    {
        AppUser user = _accountService.FindUser(CurrentUserId)!;

        // The service reads the booking itself, inside its transaction, so it sees the latest participants.
        if (!_bookingService.TryJoin(id, user, _rules.Now(), out Booking? booking, out string? error))
        {
            if (booking is null)
            {
                return NotFound();
            }
            return Conflict(new ErrorResponse("You cannot join this booking.", [error]));
        }

        if (booking.HasRequested(user.Id))
        {
            _logger.LogInformation("{UserId} asked to join booking {BookingId}; notify the owner", user.Id, booking.Id);
        }
        else
        {
            _logger.LogInformation("{UserId} joined booking {BookingId}; notify the owner", user.Id, booking.Id);
        }
        return Ok(BookingResponse.From(booking, CurrentUserId));
    }

    // POST /bookings/1/leave
    [HttpPost("{id}/leave")]
    public IActionResult Leave(int id)
    {
        Booking? booking = _bookingService.FindBooking(id);
        if (booking is null)
        {
            return NotFound();
        }

        if (!_bookingService.TryLeave(booking, CurrentUserId, _rules.Now(), out string? error))
        {
            return Conflict(new ErrorResponse("You cannot leave this booking.", [error]));
        }

        _logger.LogInformation("{UserId} left booking {BookingId}; notify the owner", CurrentUserId, booking.Id);
        return Ok(BookingResponse.From(booking, CurrentUserId));
    }

    // POST /bookings/1/participants/{userId}/approve, sent by the owner
    [HttpPost("{id}/participants/{participantId}/approve")]
    public IActionResult Approve(int id, string participantId)
    {
        if (!_bookingService.TryApprove(id, CurrentUserId, participantId, _rules.Now(), out Booking? booking, out string? error))
        {
            if (booking is null)
            {
                return NotFound();
            }
            if (booking.OwnerId != CurrentUserId)
            {
                return Forbidden("You cannot approve requests.", error);
            }
            return Conflict(new ErrorResponse("The request cannot be approved.", [error]));
        }

        _logger.LogInformation("{ParticipantId} approved for booking {BookingId}; notify them", participantId, booking.Id);
        return Ok(BookingResponse.From(booking, CurrentUserId));
    }

    // POST /bookings/1/participants/{userId}/decline, sent by the owner
    [HttpPost("{id}/participants/{participantId}/decline")]
    public IActionResult Decline(int id, string participantId)
    {
        Booking? booking = _bookingService.FindBooking(id);
        if (booking is null)
        {
            return NotFound();
        }
        if (booking.OwnerId != CurrentUserId)
        {
            return Forbidden("You cannot decline requests.", "Only the owner can decline requests.");
        }

        if (!_bookingService.TryDecline(booking, participantId, _rules.Now(), out string? error))
        {
            return Conflict(new ErrorResponse("The request cannot be declined.", [error]));
        }

        _logger.LogInformation("{ParticipantId} declined for booking {BookingId}; notify them", participantId, booking.Id);
        return Ok(BookingResponse.From(booking, CurrentUserId));
    }

    // POST /bookings/1/participants/{userId}/remove, sent by the owner
    [HttpPost("{id}/participants/{participantId}/remove")]
    public IActionResult Remove(int id, string participantId)
    {
        Booking? booking = _bookingService.FindBooking(id);
        if (booking is null)
        {
            return NotFound();
        }
        if (booking.OwnerId != CurrentUserId)
        {
            return Forbidden("You cannot remove people.", "Only the owner can remove people.");
        }

        if (!_bookingService.TryRemove(booking, participantId, _rules.Now(), out string? error))
        {
            return Conflict(new ErrorResponse("This person cannot be removed.", [error]));
        }

        _logger.LogInformation("{ParticipantId} removed from booking {BookingId}; notify them", participantId, booking.Id);
        return Ok(BookingResponse.From(booking, CurrentUserId));
    }

    // POST /bookings/1/cancel, sent by the owner
    [HttpPost("{id}/cancel")]
    public IActionResult Cancel(int id)
    {
        Booking? booking = _bookingService.FindBooking(id);
        if (booking is null)
        {
            return NotFound();
        }
        if (booking.OwnerId != CurrentUserId)
        {
            return Forbidden("You cannot cancel this booking.", "Only the owner can cancel the booking.");
        }

        if (!_bookingService.TryCancel(booking, _rules.Now(), _rules.CancelCutoff, out string? error))
        {
            return Conflict(new ErrorResponse("The booking cannot be cancelled.", [error]));
        }

        _logger.LogInformation("Booking {BookingId} cancelled by its owner; people to notify: {Count}",
            booking.Id, booking.CountJoined() - 1);
        return Ok(BookingResponse.From(booking, CurrentUserId));
    }
}
