namespace StudyRooms.Api.Domain;

public class Participant
{
    public string UserId { get; }
    public ParticipantStatus Status { get; private set; }
    public DateTime CreatedAt { get; }

    public Participant(string userId, ParticipantStatus status)
    {
        UserId = userId;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }
}
