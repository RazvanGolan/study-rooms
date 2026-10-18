using StudyRooms.Api.Domain;

namespace StudyRooms.Api.Models;

// The JSON body of POST /bookings. The owner is whoever is logged in.
public record BookingRequest(int RoomId, DateOnly Date, TimeOnly Start, TimeOnly End, JoinPolicy JoinPolicy);
