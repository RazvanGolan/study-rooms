using StudyRooms.Api.Domain;
using StudyRooms.Api.Services;

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
    public static BookingResponse From(Booking booking, BookingService bookingService)
    {
        Room room = bookingService.GetRoom(booking.RoomId);

        List<string> people = [];
        foreach (Participant participant in booking.Participants)
        {
            if (participant.Status == ParticipantStatus.Joined)
            {
                people.Add(bookingService.GetUser(participant.UserId).DisplayName);
            }
        }

        return new BookingResponse(
            booking.Id,
            room.Id,
            room.Name,
            booking.Date,
            booking.Start,
            booking.End,
            bookingService.GetUser(booking.OwnerId).DisplayName,
            people,
            Math.Max(0, room.Capacity - people.Count),
            booking.JoinPolicy,
            booking.Status,
            booking.Reason);
    }
}
