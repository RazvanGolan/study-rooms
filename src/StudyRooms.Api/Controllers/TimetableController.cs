using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Domain;
using StudyRooms.Api.Models;
using StudyRooms.Api.Services;

namespace StudyRooms.Api.Controllers;

[ApiController]
[Route("timetable")]
public class TimetableController : ControllerBase
{
    private readonly BookingService _bookingService;

    public TimetableController(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // GET /timetable?date=2026-10-20
    [HttpGet]
    public List<BookingResponse> Get(DateOnly date)
    {
        List<BookingResponse> bookings = [];
        foreach (Booking booking in _bookingService.GetBookings(date))
        {
            bookings.Add(BookingResponse.From(booking, _bookingService));
        }
        return bookings;
    }
}
