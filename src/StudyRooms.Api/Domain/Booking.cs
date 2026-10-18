using System.Diagnostics.CodeAnalysis;

namespace StudyRooms.Api.Domain;

public class Booking
{
    public int Id { get; private set; }    // the database picks it when the booking is first saved
    public int RoomId { get; private set; }

    // A navigation property: the Room object itself, next to its id. EF Core fills it in when the
    // query asks for it with Include. "= null!" tells the compiler that it will not stay null.
    public Room Room { get; private set; } = null!;

    public string? OwnerId { get; private set; }    // null for a block
    public AppUser? Owner { get; private set; }
    public string? Note { get; private set; }       // only on a block, such as "Class: PCLP"
    public DateOnly Date { get; private set; }
    public TimeOnly Start { get; private set; }
    public TimeOnly End { get; private set; }
    public JoinPolicy JoinPolicy { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Active;
    public CancelReason? Reason { get; private set; }    // why it was cancelled, if it was
    public List<Participant> Participants { get; } = [];

    // A booking made by a student, who becomes its owner.
    public Booking(Room room, AppUser owner, DateOnly date, TimeOnly start, TimeOnly end, JoinPolicy joinPolicy)
        : this(room, date, start, end)
    {
        Owner = owner;
        OwnerId = owner.Id;
        JoinPolicy = joinPolicy;

        // The owner is a participant too, so "everyone in the booking" always includes them.
        Participants.Add(new Participant(owner, ParticipantStatus.Joined));
    }

    // A block made by an admin, for a class or for maintenance. It has no owner and nobody can join it.
    public Booking(Room room, DateOnly date, TimeOnly start, TimeOnly end, string note)
        : this(room, date, start, end)
    {
        Note = note;
        JoinPolicy = JoinPolicy.Private;
    }

    // What both kinds of booking have in common. ": this(...)" above calls it first.
    private Booking(Room room, DateOnly date, TimeOnly start, TimeOnly end)
    {
        if (end <= start)
        {
            throw new ArgumentException("A booking must end after it starts.", nameof(end));
        }

        Room = room;
        RoomId = room.Id;
        Date = date;
        Start = start;
        End = end;
    }

    // EF Core uses this constructor when it reads a booking from the database, then fills in
    // every property and the participants. The ones above would add the owner a second time.
    private Booking()
    {
    }

    public DateTime StartsAt => Date.ToDateTime(Start);

    public bool IsBlock => OwnerId is null;

    public bool HasJoined(string userId)
    {
        Participant? participant = FindParticipant(userId);
        return participant is not null && participant.Status == ParticipantStatus.Joined;
    }

    public bool HasRequested(string userId)
    {
        Participant? participant = FindParticipant(userId);
        return participant is not null && participant.Status == ParticipantStatus.Requested;
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

    // The room's capacity may have been lowered since people joined, so this is >= and not ==.
    public bool IsFull()
    {
        return CountJoined() >= Room.Capacity;
    }

    public Participant? FindParticipant(string userId)
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

    // Joins an open booking at once. On a booking with the "on request" policy it only asks:
    // the student is in only after the owner approves.
    public bool TryJoin(AppUser user, DateTime now, [NotNullWhen(false)] out string? error)
    {
        if (!IsOpenForChanges(now, out error))
        {
            return false;
        }
        if (IsBlock)
        {
            error = "Nobody can join a block.";
            return false;
        }
        if (JoinPolicy == JoinPolicy.Private)
        {
            error = "This booking is private, so nobody else can join.";
            return false;
        }

        Participant? participant = FindParticipant(user.Id);
        if (participant is not null && participant.Status == ParticipantStatus.Joined)
        {
            error = "You are already in this booking.";
            return false;
        }
        if (participant is not null && participant.Status == ParticipantStatus.Requested)
        {
            error = "You have already asked to join. The owner has not answered yet.";
            return false;
        }
        if (participant is not null && participant.Status == ParticipantStatus.Removed)
        {
            error = "The owner removed you from this booking.";
            return false;
        }
        if (participant is not null && participant.Status == ParticipantStatus.Declined)
        {
            error = "The owner declined your request.";
            return false;
        }
        if (IsFull())
        {
            error = "The booking is full.";
            return false;
        }

        // Someone who left earlier comes back in the same row.
        if (participant is null)
        {
            participant = new Participant(user, ParticipantStatus.Joined);
            Participants.Add(participant);
        }

        if (JoinPolicy == JoinPolicy.OnRequest)
        {
            participant.MarkRequested();
        }
        else
        {
            participant.MarkJoined();
        }
        return true;
    }

    public bool TryApprove(string userId, DateTime now, [NotNullWhen(false)] out string? error)
    {
        if (!IsOpenForChanges(now, out error))
        {
            return false;
        }
        if (!HasRequested(userId))
        {
            error = "This student has not asked to join.";
            return false;
        }
        if (IsFull())
        {
            error = "The booking is full.";
            return false;
        }

        FindParticipant(userId)!.MarkJoined();
        return true;
    }

    public bool TryDecline(string userId, DateTime now, [NotNullWhen(false)] out string? error)
    {
        if (!IsOpenForChanges(now, out error))
        {
            return false;
        }
        if (!HasRequested(userId))
        {
            error = "This student has not asked to join.";
            return false;
        }

        FindParticipant(userId)!.MarkDeclined();
        return true;
    }

    // When a booking fills up or is cancelled, nobody waiting can get in any more.
    // Returns the requests it declined, so that the requesters can be told.
    public List<Participant> DeclinePendingRequests()
    {
        List<Participant> declined = [];
        foreach (Participant participant in Participants)
        {
            if (participant.Status == ParticipantStatus.Requested)
            {
                participant.MarkDeclined();
                declined.Add(participant);
            }
        }
        return declined;
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
        if (!HasJoined(userId))
        {
            error = "You are not in this booking.";
            return false;
        }

        FindParticipant(userId)!.MarkLeft();
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
        if (!HasJoined(participantId))
        {
            error = "This student is not in the booking.";
            return false;
        }

        FindParticipant(participantId)!.MarkRemoved();
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
            error = $"A booking can be cancelled until {cutoff.TotalHours} hours before it starts. Ask an admin to cancel it now.";
            return false;
        }

        Cancel(CancelReason.ByOwner);
        error = null;
        return true;
    }

    // An admin can cancel any booking, at any time.
    public bool TryCancelAsAdmin([NotNullWhen(false)] out string? error)
    {
        if (Status == BookingStatus.Cancelled)
        {
            error = "The booking is already cancelled.";
            return false;
        }

        Cancel(CancelReason.ByAdmin);
        error = null;
        return true;
    }

    public void CancelBecauseRoomRemoved()
    {
        Cancel(CancelReason.RoomRemoved);
    }

    private void Cancel(CancelReason reason)
    {
        Status = BookingStatus.Cancelled;
        Reason = reason;
        DeclinePendingRequests();
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
