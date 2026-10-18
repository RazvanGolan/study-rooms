using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StudyRooms.Api.Domain;
using StudyRooms.Api.Models;
using StudyRooms.Api.Services;

namespace StudyRooms.Api.Controllers;

// Sign up, log in, and get a new access token when the old one expires.
// No [Authorize] here: you cannot have a token before you log in.
[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;
    private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;

    public AccountController(AccountService accountService, IOptionsMonitor<BearerTokenOptions> bearerTokenOptions)
    {
        _accountService = accountService;
        _bearerTokenOptions = bearerTokenOptions;
    }

    // POST /account/register
    [HttpPost("register")]
    public IActionResult Register(RegisterRequest request)
    {
        if (!_accountService.TryRegister(request.Email, request.Password, request.DisplayName,
                out AppUser? user, out List<string> problems))
        {
            return BadRequest(new ErrorResponse("The account was not created.", problems));
        }
        return Created("/me", new MeResponse(user.Id, user.Email, user.DisplayName, user.IsAdmin));
    }

    // POST /account/login
    // The answer has an access token, to send with every request, and a refresh token, to get a new
    // access token when this one expires (after an hour).
    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        AppUser? user = _accountService.CheckPassword(request.Email, request.Password);
        if (user is null)
        {
            // The same answer for a wrong email and a wrong password, so nobody can find out
            // which emails have an account.
            return Unauthorized(new ErrorResponse("You are not logged in.", ["Wrong email or password."]));
        }
        return SignInWithToken(user);
    }

    // POST /account/refresh
    [HttpPost("refresh")]
    public IActionResult Refresh(RefreshRequest request)
    {
        // The refresh token is encrypted with a key only this server has. Unprotect decrypts it,
        // or returns null if someone made it up or changed it.
        BearerTokenOptions options = _bearerTokenOptions.Get(BearerTokenDefaults.AuthenticationScheme);
        AuthenticationTicket? ticket = options.RefreshTokenProtector.Unprotect(request.RefreshToken);
        if (ticket is null || ticket.Properties.ExpiresUtc is null || ticket.Properties.ExpiresUtc < DateTimeOffset.UtcNow)
        {
            return Unauthorized(new ErrorResponse("You are not logged in.", ["The refresh token is not valid. Log in again."]));
        }

        // Read the account again, so that a new display name or role is in the new token.
        string? userId = ticket.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        AppUser? user = null;
        if (userId is not null)
        {
            user = _accountService.FindUser(userId);
        }
        if (user is null)
        {
            return Unauthorized(new ErrorResponse("You are not logged in.", ["The account does not exist any more."]));
        }
        return SignInWithToken(user);
    }

    private IActionResult SignInWithToken(AppUser user)
    {
        string role = Roles.Student;
        if (user.IsAdmin)
        {
            role = Roles.Admin;
        }

        // Claims: what the token says about the user. CurrentUserId reads the NameIdentifier,
        // and [Authorize(Roles = ...)] reads the Role.
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, role),
        ];
        ClaimsIdentity identity = new ClaimsIdentity(claims, BearerTokenDefaults.AuthenticationScheme);

        // SignIn with the bearer token scheme encrypts the claims into the tokens and writes the answer.
        return SignIn(new ClaimsPrincipal(identity), BearerTokenDefaults.AuthenticationScheme);
    }
}
