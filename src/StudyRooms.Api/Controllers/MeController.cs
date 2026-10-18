using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Domain;
using StudyRooms.Api.Models;
using StudyRooms.Api.Services;

namespace StudyRooms.Api.Controllers;

// Everything about whoever is logged in.
[ApiController]
[Route("me")]
[Authorize]
public class MeController : AppControllerBase
{
    private readonly BookingService _bookingService;
    private readonly AccountService _accountService;

    public MeController(BookingService bookingService, AccountService accountService)
    {
        _bookingService = bookingService;
        _accountService = accountService;
    }

    // GET /me
    [HttpGet]
    public MeResponse Get()
    {
        AppUser user = _accountService.FindUser(CurrentUserId)!;
        return new MeResponse(user.Id, user.Email, user.DisplayName, user.IsAdmin);
    }

    // GET /me/bookings: the bookings I am in or have asked to join, past and future, cancelled ones too
    [HttpGet("bookings")]
    public List<BookingResponse> GetBookings()
    {
        List<BookingResponse> bookings = [];
        foreach (Booking booking in _bookingService.GetBookingsOf(CurrentUserId))
        {
            bookings.Add(BookingResponse.From(booking, CurrentUserId));
        }
        return bookings;
    }
}
