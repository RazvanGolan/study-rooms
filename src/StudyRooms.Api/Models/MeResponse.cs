namespace StudyRooms.Api.Models;

// The account of whoever is logged in. The interface uses IsAdmin to show the admin pages.
public record MeResponse(string Id, string Email, string DisplayName, bool IsAdmin);
