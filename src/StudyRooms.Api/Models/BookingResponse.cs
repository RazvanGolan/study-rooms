using StudyRooms.Api.Domain;

namespace StudyRooms.Api.Models;

// What a student sees about a booking: names, but no emails.
// Requests holds the people waiting for approval, and only the owner sees them.
// MyStatus says where the student asking stands: Joined, Requested, ..., or null if not in it.
public record BookingResponse(
    int Id,
    int RoomId,
    string Room,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    string? Owner,
    string? Note,
    List<PersonResponse> People,
    List<PersonResponse> Requests,
    int PlacesLeft,
    JoinPolicy JoinPolicy,
    BookingStatus Status,
    CancelReason? Reason,
    ParticipantStatus? MyStatus)
{
    // The booking must come with its room, owner and participants loaded (BookingService does that).
    public static BookingResponse From(Booking booking, string userId)
    {
        List<PersonResponse> people = [];
        List<PersonResponse> requests = [];
        foreach (Participant participant in booking.Participants)
        {
            PersonResponse person = new PersonResponse(participant.UserId, participant.User.DisplayName);
            if (participant.Status == ParticipantStatus.Joined)
            {
                people.Add(person);
            }
            else if (participant.Status == ParticipantStatus.Requested && userId == booking.OwnerId)
            {
                requests.Add(person);
            }
        }

        int placesLeft = 0;
        if (!booking.IsBlock)
        {
            placesLeft = Math.Max(0, booking.Room.Capacity - people.Count);
        }

        return new BookingResponse(
            booking.Id,
            booking.RoomId,
            booking.Room.Name,
            booking.Date,
            booking.Start,
            booking.End,
            booking.Owner?.DisplayName,
            booking.Note,
            people,
            requests,
            placesLeft,
            booking.JoinPolicy,
            booking.Status,
            booking.Reason,
            booking.FindParticipant(userId)?.Status);
    }
}
