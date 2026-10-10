using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Domain;
using StudyRooms.Api.Models;

namespace StudyRooms.Api.Controllers;

[ApiController]
[Route("timetable")]
public class TimetableController : ControllerBase
{
    private readonly Timetable _timetable;

    public TimetableController(Timetable timetable)
    {
        _timetable = timetable;
    }

    // GET /timetable?date=2026-10-20
    [HttpGet]
    public List<BookingResponse> Get(DateOnly date)
    {
        List<BookingResponse> bookings = [];
        foreach (Booking booking in _timetable.GetBookings(date))
        {
            bookings.Add(BookingResponse.From(booking, _timetable));
        }
        return bookings;
    }
}
