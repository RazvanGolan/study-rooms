using Microsoft.Extensions.Options;

namespace StudyRooms.Api;

// The rules about when a booking can be made, with the numbers from appsettings.json.
public class BookingRules
{
    private readonly BookingOptions _options;
    private readonly TimeZoneInfo _timeZone;

    public BookingRules(IOptions<BookingOptions> options)
    {
        _options = options.Value;
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZone);
    }

    public TimeSpan CancelCutoff => TimeSpan.FromHours(_options.CancelCutoffHours);

    // The current time at the faculty, whatever time zone the server runs in.
    public DateTime Now()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);
    }

    public List<string> CheckTimes(DateOnly date, TimeOnly start, TimeOnly end)
    {
        List<string> problems = [];

        if (end <= start)
        {
            problems.Add("A booking must end after it starts.");
            return problems;
        }

        if (!IsOnStep(start) || !IsOnStep(end))
        {
            problems.Add($"Start and end must be on a {_options.StepMinutes} minute step, like 10:00 or 10:30.");
        }

        double minutes = (end - start).TotalMinutes;
        if (minutes < _options.MinMinutes || minutes > _options.MaxMinutes)
        {
            problems.Add($"A booking lasts from {_options.MinMinutes} to {_options.MaxMinutes} minutes.");
        }

        if (start < _options.Opens || end > _options.Closes)
        {
            problems.Add($"Rooms can be booked between {_options.Opens} and {_options.Closes}.");
        }

        DateTime now = Now();
        if (date.ToDateTime(start) <= now)
        {
            problems.Add("A booking cannot start in the past.");
        }
        if (date > DateOnly.FromDateTime(now).AddDays(_options.DaysAhead))
        {
            problems.Add($"Rooms can be booked at most {_options.DaysAhead} days ahead.");
        }

        return problems;
    }

    private bool IsOnStep(TimeOnly time)
    {
        int minutesSinceMidnight = time.Hour * 60 + time.Minute;
        return time.Second == 0 && minutesSinceMidnight % _options.StepMinutes == 0;
    }
}
