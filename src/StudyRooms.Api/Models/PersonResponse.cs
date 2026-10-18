namespace StudyRooms.Api.Models;

// Someone in a booking, as other students see them. The id lets the owner remove or approve
// a person; it says nothing about them.
public record PersonResponse(string Id, string DisplayName);
