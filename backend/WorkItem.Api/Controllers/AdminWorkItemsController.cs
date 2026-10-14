using Microsoft.AspNetCore.Mvc;
using WorkItem.Api.Auth;
using WorkItem.Api.Data;
using WorkItem.Api.Models;

namespace WorkItem.Api.Controllers;

[ApiController]
[Route("api/admin/work-items")]
public sealed class AdminWorkItemsController(WorkItemRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? direction, [FromQuery] int page = 1)
    {
        var denied = Gate();
        if (denied is not null) return denied;
        return Ok(await repo.ListAdminAsync(direction, page));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id)
    {
        var denied = Gate();
        if (denied is not null) return denied;
        var item = await repo.GetAdminAsync(id);
        return item is null ? NotFound(new Error("not_found")) : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WorkItemWrite? body)
    {
        var denied = Gate();
        if (denied is not null) return denied;
        var saved = await repo.CreateAsync(body?.Title, body?.Description);
        if (saved.Error == "validation") return BadRequest(new FieldError("validation", "title"));
        return Created($"/api/admin/work-items/{saved.Item!.Id}", saved.Item);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] WorkItemWrite? body)
    {
        var denied = Gate();
        if (denied is not null) return denied;
        var saved = await repo.UpdateAsync(id, body?.Title, body?.Description);
        if (saved.Error == "validation") return BadRequest(new FieldError("validation", "title"));
        if (saved.Error == "not_found") return NotFound(new Error("not_found"));
        return Ok(saved.Item);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var denied = Gate();
        if (denied is not null) return denied;
        return await repo.DeleteAsync(id) ? NoContent() : NotFound(new Error("not_found"));
    }

    private IActionResult? Gate() => new CallerContext(Request).Require(admin: true);
}
