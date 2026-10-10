using StudyRooms.Api.Domain;

namespace StudyRooms.Api.Models;

// What other students see about a booking: names, but no ids or emails.
public record BookingResponse(
    int Id,
    int RoomId,
    string Room,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    string Owner,
    List<string> People,
    int PlacesLeft,
    JoinPolicy JoinPolicy,
    BookingStatus Status,
    CancelReason? Reason)
{
    public static BookingResponse From(Booking booking, Timetable timetable)
    {
        Room room = timetable.GetRoom(booking.RoomId);

        List<string> people = [];
        foreach (Participant participant in booking.Participants)
        {
            if (participant.Status == ParticipantStatus.Joined)
            {
                people.Add(timetable.GetUser(participant.UserId).DisplayName);
            }
        }

        return new BookingResponse(
            booking.Id,
            room.Id,
            room.Name,
            booking.Date,
            booking.Start,
            booking.End,
            timetable.GetUser(booking.OwnerId).DisplayName,
            people,
            Math.Max(0, room.Capacity - people.Count),
            booking.JoinPolicy,
            booking.Status,
            booking.Reason);
    }
}
