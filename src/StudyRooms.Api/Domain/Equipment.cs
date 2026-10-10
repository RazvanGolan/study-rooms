namespace StudyRooms.Api.Domain;

// A room can have several of these at once, so each value is one bit:
// Equipment.Projector | Equipment.Whiteboard is a room with both.
[Flags]
public enum Equipment
{
    None = 0,
    Projector = 1,
    Whiteboard = 2,
    Tv = 4,
    PowerOutlets = 8,
}
