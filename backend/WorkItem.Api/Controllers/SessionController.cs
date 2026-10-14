using Microsoft.AspNetCore.Mvc;
using WorkItem.Api.Models;

namespace WorkItem.Api.Controllers;

[ApiController]
[Route("api/session")]
public sealed class SessionController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] SignIn? body)
    {
        var name = body?.DisplayName?.Trim() ?? "";
        var role = body?.Role?.Trim() ?? "";
        if (name.Length == 0) return BadRequest(new Error("name_required"));
        if (role is not (Role.User or Role.Admin)) return BadRequest(new Error("invalid_role"));
        return Ok(new Session(name, role));
    }
}
