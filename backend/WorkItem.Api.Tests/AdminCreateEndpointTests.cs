using System.Net;
using System.Net.Http.Json;
using WorkItem.Api.Models;

namespace WorkItem.Api.Tests;

public sealed class AdminCreateEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Creates_with_equal_timestamps_and_lists_newest_first()
    {
        factory.Reset();
        var admin = factory.Client("Admin", "admin");
        var res = await admin.PostAsJsonAsync("/api/admin/work-items", new { title = " First ", description = "Notes" });
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var created = (await res.Content.ReadFromJsonAsync<AdminWorkItem>())!;
        Assert.Equal("First", created.Title);
        Assert.Equal("Notes", created.Description);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);
        await ApiFactory.CreateItem(admin, "Second");
        var page = await admin.GetFromJsonAsync<AdminWorkItemPage>("/api/admin/work-items");
        Assert.Equal("Second", page!.Items[0].Title);
        Assert.Equal("First", page.Items[1].Title);
        var userPage = await factory.Client().GetFromJsonAsync<WorkItemPage>("/api/work-items");
        Assert.All(userPage!.Items, item => Assert.Equal("pending", item.Status));
    }

    [Fact]
    public async Task Blank_title_writes_nothing()
    {
        factory.Reset();
        var admin = factory.Client("Admin", "admin");
        var res = await admin.PostAsJsonAsync("/api/admin/work-items", new { title = "   ", description = "x" });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var body = (await res.Content.ReadFromJsonAsync<FieldError>())!;
        Assert.Equal("validation", body.Code);
        Assert.Equal("title", body.Field);
        Assert.Equal(0, factory.Count("SELECT COUNT(*) FROM work_item"));
    }

    [Fact]
    public async Task Front_end_user_is_forbidden()
    {
        factory.Reset();
        var res = await factory.Client("A", "user").PostAsJsonAsync("/api/admin/work-items", new { title = "Nope" });
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        Assert.Equal("admin_required", (await res.Content.ReadFromJsonAsync<Error>())!.Code);
        Assert.Equal(0, factory.Count("SELECT COUNT(*) FROM work_item"));
    }
}
