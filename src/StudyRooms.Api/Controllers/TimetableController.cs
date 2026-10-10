using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Domain;

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
    public List<Booking> Get(DateOnly date)
    {
        return _timetable.GetBookings(date);
    }
}
