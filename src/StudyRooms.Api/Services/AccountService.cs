using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyRooms.Api.Data;
using StudyRooms.Api.Domain;

namespace StudyRooms.Api.Services;

// Accounts: signing up and checking a password.
public class AccountService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<AppUser> _passwordHasher;

    public AccountService(AppDbContext db, PasswordHasher<AppUser> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public AppUser? FindUser(string id)
    {
        return _db.Users.Find(id);
    }

    public bool TryRegister(string email, string password, string displayName,
        [NotNullWhen(true)] out AppUser? user, out List<string> problems)
    {
        user = null;
        email = NormalizeEmail(email);
        problems = [];

        if (!email.Contains('@'))
        {
            problems.Add("Enter a valid email address.");
        }
        if (password.Length < 8)
        {
            problems.Add("The password must have at least 8 characters.");
        }
        if (string.IsNullOrWhiteSpace(displayName))
        {
            problems.Add("Choose a display name.");
        }
        if (FindByEmail(email) is not null)
        {
            problems.Add("There is already an account with this email.");
        }
        if (problems.Count > 0)
        {
            return false;
        }

        user = new AppUser(Guid.NewGuid().ToString(), email, displayName.Trim(), false);
        user.SetPasswordHash(_passwordHasher.HashPassword(user, password));
        _db.Users.Add(user);

        // Two sign-ups with the same email at the same moment can both pass the check above.
        // The unique index on Email refuses the second one.
        try
        {
            _db.SaveChanges();
        }
        catch (DbUpdateException)
        {
            problems.Add("There is already an account with this email.");
            user = null;
            return false;
        }
        return true;
    }

    // The account, if the email exists and the password matches its hash. Otherwise null.
    public AppUser? CheckPassword(string email, string password)
    {
        AppUser? user = FindByEmail(email);
        if (user is null)
        {
            return null;
        }

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }
        return user;
    }

    public AppUser? FindByEmail(string email)
    {
        string normalized = NormalizeEmail(email);
        return _db.Users.FirstOrDefault(u => u.Email == normalized);
    }

    // Emails are stored in lowercase, so Ana@Example.com and ana@example.com are the same account.
    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
