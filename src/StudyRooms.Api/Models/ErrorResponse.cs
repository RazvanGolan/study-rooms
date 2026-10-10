namespace StudyRooms.Api.Models;

// The body of every error this API sends: a short title and the reasons.
public record ErrorResponse(string Title, List<string> Problems);
