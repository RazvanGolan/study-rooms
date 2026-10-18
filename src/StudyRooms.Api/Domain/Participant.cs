using Microsoft.EntityFrameworkCore;

namespace StudyRooms.Api.Domain;

// A student appears at most once in a booking, so the booking and the user together are the key.
[PrimaryKey(nameof(BookingId), nameof(UserId))]
public class Participant
{
    public int BookingId { get; private set; }    // filled in when the booking is saved
    public string UserId { get; private set; }
    public ParticipantStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Participant(string userId, ParticipantStatus status)
    {
        UserId = userId;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkJoined()
    {
        Status = ParticipantStatus.Joined;
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
