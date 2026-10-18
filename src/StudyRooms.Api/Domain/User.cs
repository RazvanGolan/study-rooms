namespace StudyRooms.Api.Domain;

// A student. Other students only ever see the display name.
// Accounts with passwords replace this class later in the semester.
public class User
{
    public string Id { get; private set; }
    public string DisplayName { get; private set; }
    public string Email { get; private set; }

    public User(string id, string displayName, string email)
    {
        Id = id;
        DisplayName = displayName;
        Email = email;
    }
}
