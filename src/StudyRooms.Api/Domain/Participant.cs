using Microsoft.EntityFrameworkCore;

namespace StudyRooms.Api.Domain;

// A student appears at most once in a booking, so the booking and the user together are the key.
[PrimaryKey(nameof(BookingId), nameof(UserId))]
public class Participant
{
    public int BookingId { get; private set; }    // filled in when the booking is saved
    public string UserId { get; private set; }
    public AppUser User { get; private set; } = null!;    // EF Core fills it in, see Booking.Room
    public ParticipantStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Participant(AppUser user, ParticipantStatus status)
    {
        User = user;
        UserId = user.Id;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    // EF Core uses this constructor when it reads a participant from the database.
    private Participant()
    {
        UserId = "";
    }

    public void MarkJoined()
    {
        Status = ParticipantStatus.Joined;
    }

    public void MarkRequested()
    {
        Status = ParticipantStatus.Requested;
    }

    public void MarkDeclined()
    {
        Status = ParticipantStatus.Declined;
    }

    public void MarkLeft()
    {
        Status = ParticipantStatus.Left;
    }

    public void MarkRemoved()
    {
        Status = ParticipantStatus.Removed;
    }
}
