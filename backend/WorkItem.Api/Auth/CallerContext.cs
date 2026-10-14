using Microsoft.AspNetCore.Mvc;
using WorkItem.Api.Models;

namespace WorkItem.Api.Auth;

public sealed class CallerContext
{
    public string? DisplayName { get; }
    public string Role { get; }

    public CallerContext(HttpRequest request)
    {
        var name = request.Headers["X-Display-Name"].ToString().Trim();
        DisplayName = name.Length == 0 ? null : name;
        Role = request.Headers["X-Role"].ToString().Trim();
    }

    public IActionResult? Require(bool admin = false)
    {
        if (DisplayName is null)
            return new UnauthorizedObjectResult(new Error("sign_in_required"));
        if (admin && Role != Models.Role.Admin)
            return new ObjectResult(new Error("admin_required")) { StatusCode = StatusCodes.Status403Forbidden };
        return null;
    }
}
