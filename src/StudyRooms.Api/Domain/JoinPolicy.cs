namespace StudyRooms.Api.Domain;

// R8: who else can join a booking.
public enum JoinPolicy
{
    Private,
    Open,
    OnRequest,
}
