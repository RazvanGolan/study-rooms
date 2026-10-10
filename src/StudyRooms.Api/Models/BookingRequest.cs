using StudyRooms.Api.Domain;

namespace StudyRooms.Api.Models;

// The JSON body of POST /bookings. Until we have accounts, the request says who is booking.
public record BookingRequest(int RoomId, DateOnly Date, TimeOnly Start, TimeOnly End, string UserId, JoinPolicy JoinPolicy);
