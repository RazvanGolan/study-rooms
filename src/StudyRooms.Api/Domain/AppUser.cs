using Microsoft.EntityFrameworkCore;

namespace StudyRooms.Api.Domain;

// An account. Other students only ever see the display name.
// It is not called User because every controller already has a property called User:
// what the token says about whoever sent the request.
[Index(nameof(Email), IsUnique = true)]
public class AppUser
{
    public string Id { get; private set; }
    public string Email { get; private set; }
    public string DisplayName { get; private set; }
    public string PasswordHash { get; private set; } = "";    // never the password itself
    public bool IsAdmin { get; private set; }

    public AppUser(string id, string email, string displayName, bool isAdmin)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        IsAdmin = isAdmin;
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
