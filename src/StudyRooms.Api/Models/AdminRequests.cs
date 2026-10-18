using StudyRooms.Api.Domain;

namespace StudyRooms.Api.Models;

// POST /admin/rooms and PUT /admin/rooms/1
public record RoomRequest(string Name, string Building, int Capacity, Equipment Equipment);

// POST /admin/bookings: a booking for a student, who becomes its owner
public record AdminBookingRequest(int RoomId, DateOnly Date, TimeOnly Start, TimeOnly End, string StudentEmail, JoinPolicy JoinPolicy);

// POST /admin/bookings/blocks: the room is taken for a class or for maintenance
public record BlockRequest(int RoomId, DateOnly Date, TimeOnly Start, TimeOnly End, string Note);
