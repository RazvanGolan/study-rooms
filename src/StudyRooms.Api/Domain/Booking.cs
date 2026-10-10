namespace StudyRooms.Api.Domain;

public class Booking
{
    public int Id { get; }
    public int RoomId { get; }
    public string OwnerId { get; }
    public DateOnly Date { get; }
    public TimeOnly Start { get; }
    public TimeOnly End { get; }
    public JoinPolicy JoinPolicy { get; }
    public BookingStatus Status { get; private set; } = BookingStatus.Active;
    public List<Participant> Participants { get; } = [];

    public Booking(int id, int roomId, string ownerId, DateOnly date, TimeOnly start, TimeOnly end, JoinPolicy joinPolicy)
    {
        if (end <= start)
        {
            throw new ArgumentException("A booking must end after it starts.", nameof(end));
        }

        Id = id;
        RoomId = roomId;
        OwnerId = ownerId;
        Date = date;
        Start = start;
        End = end;
        JoinPolicy = joinPolicy;

        // The owner is a participant too, so "everyone in the booking" always includes them.
        Participants.Add(new Participant(ownerId, ParticipantStatus.Joined));
    }

    // Two time ranges overlap when each one starts before the other ends.
    // 10:00-11:00 and 11:00-12:00 only touch, so they do not overlap.
    public bool Overlaps(DateOnly date, TimeOnly start, TimeOnly end)
    {
        return Date == date && Start < end && start < End;
    }

    public bool HasJoined(string userId)
    {
        foreach (Participant participant in Participants)
        {
            if (participant.UserId == userId && participant.Status == ParticipantStatus.Joined)
            {
                return true;
            }
        }
        return false;
    }
}
