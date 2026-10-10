namespace StudyRooms.Api;

// The "Booking" section of appsettings.json. Change the numbers there, not in the code.
public class BookingOptions
{
    public int StepMinutes { get; set; }
    public int MinMinutes { get; set; }
    public int MaxMinutes { get; set; }
    public TimeOnly Opens { get; set; }
    public TimeOnly Closes { get; set; }
    public int DaysAhead { get; set; }
    public int CancelCutoffHours { get; set; }
    public string TimeZone { get; set; } = "";
}
