using System.Net;
using System.Net.Http.Json;
using WorkItem.Api.Models;

namespace WorkItem.Api.Tests;

public sealed class AdminEditEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Updates_the_title_and_updated_at_without_changing_confirmation()
    {
        factory.Reset();
        var admin = factory.Client("Admin", "admin");
        var user = factory.Client("A");
        var created = await ApiFactory.CreateItem(admin, "Old", "Desc");
        await user.PostAsJsonAsync("/api/work-items/confirm", new { ids = new[] { created.Id } });
        await Task.Delay(30);
        var blank = await admin.PutAsJsonAsync($"/api/admin/work-items/{created.Id}", new { title = "   ", description = "nope" });
        Assert.Equal(HttpStatusCode.BadRequest, blank.StatusCode);
        Assert.Equal("title", (await blank.Content.ReadFromJsonAsync<FieldError>())!.Field);
        var unchanged = await admin.GetFromJsonAsync<AdminWorkItem>($"/api/admin/work-items/{created.Id}");
        Assert.Equal("Old", unchanged!.Title);
        Assert.Equal("Desc", unchanged.Description);
        Assert.Equal(created.UpdatedAt, unchanged.UpdatedAt);
        var res = await admin.PutAsJsonAsync($"/api/admin/work-items/{created.Id}", new { title = " New ", description = "Next" });
        var updated = (await res.Content.ReadFromJsonAsync<AdminWorkItem>())!;
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("New", updated.Title);
        Assert.Equal("Next", updated.Description);
        Assert.NotEqual(created.UpdatedAt, updated.UpdatedAt);
        var detail = await user.GetFromJsonAsync<WorkItemDetail>($"/api/work-items/{created.Id}");
        Assert.Equal("confirmed", detail!.Status);
        Assert.Equal(1, factory.Count("SELECT COUNT(*) FROM confirmation"));
    }

    [Fact]
    public async Task Missing_id_is_not_found()
    {
        factory.Reset();
        var res = await factory.Client("Admin", "admin").PutAsJsonAsync("/api/admin/work-items/999999", new { title = "New" });
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }
}
