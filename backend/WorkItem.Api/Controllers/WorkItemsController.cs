using Microsoft.AspNetCore.Mvc;
using WorkItem.Api.Auth;
using WorkItem.Api.Data;
using WorkItem.Api.Models;

namespace WorkItem.Api.Controllers;

[ApiController]
[Route("api/work-items")]
public sealed class WorkItemsController(WorkItemRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? direction, [FromQuery] int page = 1)
    {
        var caller = new CallerContext(Request);
        var denied = caller.Require();
        if (denied is not null) return denied;
        return Ok(await repo.ListAsync(caller.DisplayName!, direction, page));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id)
    {
        var caller = new CallerContext(Request);
        var denied = caller.Require();
        if (denied is not null) return denied;
        var item = await repo.GetAsync(caller.DisplayName!, id);
        return item is null ? NotFound(new Error("not_found")) : Ok(item);
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmRequest? body)
    {
        var caller = new CallerContext(Request);
        var denied = caller.Require();
        if (denied is not null) return denied;
        if (body?.Ids is null || body.Ids.Length == 0) return BadRequest(new Error("empty_selection"));
        var count = await repo.ConfirmAsync(caller.DisplayName!, body.Ids);
        return count is null ? NotFound(new Error("not_found")) : Ok(new ConfirmResult(count.Value));
    }

    [HttpPost("{id:long}/revoke")]
    public async Task<IActionResult> Revoke(long id)
    {
        var caller = new CallerContext(Request);
        var denied = caller.Require();
        if (denied is not null) return denied;
        var ok = await repo.RevokeAsync(caller.DisplayName!, id);
        return ok ? Ok(new RevokeResult(1)) : NotFound(new Error("not_found"));
    }
}
