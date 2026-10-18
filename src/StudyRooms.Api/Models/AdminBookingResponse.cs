using StudyRooms.Api.Domain;

namespace StudyRooms.Api.Models;

// What an admin sees about a booking: everything, emails included, and every participant with their status.
public record AdminBookingResponse(
    int Id,
    int RoomId,
    string Room,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    string? OwnerEmail,
    string? Note,
    List<AdminParticipantResponse> Participants,
    JoinPolicy JoinPolicy,
    BookingStatus Status,
    CancelReason? Reason)
{
    public static AdminBookingResponse From(Booking booking)
    {
        List<AdminParticipantResponse> participants = [];
        foreach (Participant participant in booking.Participants)
        {
            participants.Add(new AdminParticipantResponse(
                participant.UserId, participant.User.DisplayName, participant.User.Email, participant.Status));
        }

        return new AdminBookingResponse(
            booking.Id,
            booking.RoomId,
            booking.Room.Name,
            booking.Date,
            booking.Start,
            booking.End,
            booking.Owner?.Email,
            booking.Note,
            participants,
            booking.JoinPolicy,
            booking.Status,
            booking.Reason);
    }
}

public record AdminParticipantResponse(string Id, string DisplayName, string Email, ParticipantStatus Status);
