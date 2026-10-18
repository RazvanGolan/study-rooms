using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StudyRooms.Api.Models;

namespace StudyRooms.Api.Controllers;

// The base class of our controllers. It adds what every one of them needs on top of ControllerBase.
public abstract class AppControllerBase : ControllerBase
{
    // The id of whoever sent the request. It comes from the token, so nobody can pretend to be someone else.
    // [Authorize] on the controller makes sure there is a logged-in user.
    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Only call this when the request has a user.");

    protected ObjectResult Forbidden(string title, string problem)
    {
        return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(title, [problem]));
    }
}
