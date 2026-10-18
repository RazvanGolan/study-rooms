using System.Diagnostics.CodeAnalysis;

namespace StudyRooms.Api.Domain;

public class Booking
{
    public int Id { get; private set; }    // the database picks it when the booking is first saved
    public int RoomId { get; private set; }
    public string OwnerId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly Start { get; private set; }
    public TimeOnly End { get; private set; }
    public JoinPolicy JoinPolicy { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Active;
    public CancelReason? Reason { get; private set; }    // why it was cancelled, if it was
    public List<Participant> Participants { get; } = [];

    public Booking(int roomId, string ownerId, DateOnly date, TimeOnly start, TimeOnly end, JoinPolicy joinPolicy)
    {
        if (end <= start)
        {
            throw new ArgumentException("A booking must end after it starts.", nameof(end));
        }

        RoomId = roomId;
        OwnerId = ownerId;
        Date = date;
        Start = start;
        End = end;
        JoinPolicy = joinPolicy;

        // The owner is a participant too, so "everyone in the booking" always includes them.
        Participants.Add(new Participant(ownerId, ParticipantStatus.Joined));
    }

    // EF Core uses this constructor when it reads a booking from the database, then fills in
    // every property and the participants. The one above would add the owner a second time.
    private Booking()
    {
        OwnerId = "";
    }

    public DateTime StartsAt => Date.ToDateTime(Start);

    public bool HasJoined(string userId)
    {
        Participant? participant = FindParticipant(userId);
        return participant is not null && participant.Status == ParticipantStatus.Joined;
    }

    public int CountJoined()
    {
        int count = 0;
        foreach (Participant participant in Participants)
        {
            if (participant.Status == ParticipantStatus.Joined)
            {
                count++;
            }
        }
        return count;
    }

    public bool TryJoin(string userId, int capacity, DateTime now, [NotNullWhen(false)] out string? error)
    {
        if (!IsOpenForChanges(now, out error))
        {
            return false;
        }
        if (JoinPolicy == JoinPolicy.Private)
        {
            error = "This booking is private.";
            return false;
        }
        if (JoinPolicy == JoinPolicy.OnRequest)
        {
            error = "The owner approves each person who joins this booking.";
            return false;
        }

        Participant? participant = FindParticipant(userId);
        if (participant is not null && participant.Status == ParticipantStatus.Joined)
        {
            error = $"{userId} is already in this booking.";
            return false;
        }
        if (participant is not null && participant.Status == ParticipantStatus.Removed)
        {
            error = "The owner removed you from this booking.";
            return false;
        }
        if (CountJoined() >= capacity)
        {
            error = "The booking is full.";
            return false;
        }

        if (participant is null)
        {
            Participants.Add(new Participant(userId, ParticipantStatus.Joined));
        }
        else
        {
            // Someone who left earlier can come back.
            participant.MarkJoined();
        }
        return true;
    }

    public bool TryLeave(string userId, DateTime now, [NotNullWhen(false)] out string? error)
    {
        if (userId == OwnerId)
        {
            error = "The owner cannot leave. Cancel the booking instead.";
            return false;
        }
        if (!IsOpenForChanges(now, out error))
        {
            return false;
        }

        Participant? participant = FindParticipant(userId);
        if (participant is null || participant.Status != ParticipantStatus.Joined)
        {
            error = $"{userId} is not in this booking.";
            return false;
        }

        participant.MarkLeft();
        return true;
    }

    public bool TryRemove(string participantId, DateTime now, [NotNullWhen(false)] out string? error)
    {
        if (participantId == OwnerId)
        {
            error = "The owner cannot be removed.";
            return false;
        }
        if (!IsOpenForChanges(now, out error))
        {
            return false;
        }

        Participant? participant = FindParticipant(participantId);
        if (participant is null || participant.Status != ParticipantStatus.Joined)
        {
            error = $"{participantId} is not in this booking.";
            return false;
        }

        participant.MarkRemoved();
        return true;
    }

    // The owner can cancel only up to some time before the start.
    public bool TryCancel(DateTime now, TimeSpan cutoff, [NotNullWhen(false)] out string? error)
    {
        if (Status == BookingStatus.Cancelled)
        {
            error = "The booking is already cancelled.";
            return false;
        }
        if (now > StartsAt - cutoff)
        {
            error = $"A booking can be cancelled until {cutoff.TotalHours} hours before it starts.";
            return false;
        }

        Status = BookingStatus.Cancelled;
        Reason = CancelReason.ByOwner;
        error = null;
        return true;
    }

    private Participant? FindParticipant(string userId)
    {
        foreach (Participant participant in Participants)
        {
            if (participant.UserId == userId)
            {
                return participant;
            }
        }
        return null;
    }

    // People can join, leave or be removed only while the booking is active and has not started.
    private bool IsOpenForChanges(DateTime now, [NotNullWhen(false)] out string? error)
    {
        if (Status == BookingStatus.Cancelled)
        {
            error = "The booking is cancelled.";
            return false;
        }
        if (now >= StartsAt)
        {
            error = "The booking has already started.";
            return false;
        }

        error = null;
        return true;
    }
}
