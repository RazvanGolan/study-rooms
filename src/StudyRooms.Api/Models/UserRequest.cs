namespace StudyRooms.Api.Models;

// The body of join, leave, remove and cancel: who is doing it. Accounts replace this later.
public record UserRequest(string UserId);
