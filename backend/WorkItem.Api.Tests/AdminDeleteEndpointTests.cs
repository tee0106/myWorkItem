using System.Net;
using System.Net.Http.Json;
using WorkItem.Api.Models;

namespace WorkItem.Api.Tests;

public sealed class AdminDeleteEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Deletes_the_item_and_its_confirmations_and_keeps_the_other_id()
    {
        factory.Reset();
        var admin = factory.Client("Admin", "admin");
        var user = factory.Client("A");
        var first = await ApiFactory.CreateItem(admin, "First");
        var second = await ApiFactory.CreateItem(admin, "Second");
        await user.PostAsJsonAsync("/api/work-items/confirm", new { ids = new[] { first.Id } });
        var res = await admin.DeleteAsync($"/api/admin/work-items/{first.Id}");
        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.Equal(0, factory.Count("SELECT COUNT(*) FROM confirmation"));
        var page = await admin.GetFromJsonAsync<AdminWorkItemPage>("/api/admin/work-items");
        Assert.Equal(second.Id, page!.Items[0].Id);
        var gone = await user.GetAsync($"/api/work-items/{first.Id}");
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }

    [Fact]
    public async Task Missing_id_is_not_found()
    {
        factory.Reset();
        var res = await factory.Client("Admin", "admin").DeleteAsync("/api/admin/work-items/999999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Front_end_user_is_forbidden()
    {
        factory.Reset();
        var created = await ApiFactory.CreateItem(factory.Client("Admin", "admin"), "Keep");
        var res = await factory.Client("A", "user").DeleteAsync($"/api/admin/work-items/{created.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        Assert.Equal(1, factory.Count("SELECT COUNT(*) FROM work_item"));
    }
}
