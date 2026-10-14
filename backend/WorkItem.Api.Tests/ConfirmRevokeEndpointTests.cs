using System.Net;
using System.Net.Http.Json;
using WorkItem.Api.Models;

namespace WorkItem.Api.Tests;

public sealed class ConfirmRevokeEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Counts_new_and_already_confirmed_ids_without_changing_updated_at()
    {
        factory.Reset();
        var admin = factory.Client("Admin", "admin");
        var user = factory.Client();
        var item = await ApiFactory.CreateItem(admin, "One");
        var again = await user.PostAsJsonAsync("/api/work-items/confirm", new { ids = new[] { item.Id } });
        var first = await again.Content.ReadFromJsonAsync<ConfirmResult>();
        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
        Assert.Equal(1, first!.ConfirmedCount);
        var secondRes = await user.PostAsJsonAsync("/api/work-items/confirm", new { ids = new[] { item.Id } });
        var second = await secondRes.Content.ReadFromJsonAsync<ConfirmResult>();
        Assert.Equal(1, second!.ConfirmedCount);
        var detail = await user.GetFromJsonAsync<WorkItemDetail>($"/api/work-items/{item.Id}");
        Assert.Equal(item.UpdatedAt, detail!.UpdatedAt);
        Assert.Equal("confirmed", detail.Status);
    }

    [Fact]
    public async Task Rejects_an_empty_selection()
    {
        factory.Reset();
        var res = await factory.Client().PostAsJsonAsync("/api/work-items/confirm", new { ids = Array.Empty<long>() });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("empty_selection", (await res.Content.ReadFromJsonAsync<Error>())!.Code);
    }

    [Fact]
    public async Task Missing_id_writes_nothing()
    {
        factory.Reset();
        var admin = factory.Client("Admin", "admin");
        var user = factory.Client();
        var item = await ApiFactory.CreateItem(admin, "Keep");
        var res = await user.PostAsJsonAsync("/api/work-items/confirm", new { ids = new[] { item.Id, 999999L } });
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var page = await user.GetFromJsonAsync<WorkItemPage>("/api/work-items");
        Assert.Equal("pending", page!.Items[0].Status);
        Assert.Equal(0, factory.Count("SELECT COUNT(*) FROM confirmation"));
    }

    [Fact]
    public async Task Revoke_returns_pending_and_keeps_updated_at()
    {
        factory.Reset();
        var admin = factory.Client("Admin", "admin");
        var user = factory.Client();
        var item = await ApiFactory.CreateItem(admin, "One");
        await user.PostAsJsonAsync("/api/work-items/confirm", new { ids = new[] { item.Id } });
        var res = await user.PostAsync($"/api/work-items/{item.Id}/revoke", null);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal(1, (await res.Content.ReadFromJsonAsync<RevokeResult>())!.RevokedCount);
        var detail = await user.GetFromJsonAsync<WorkItemDetail>($"/api/work-items/{item.Id}");
        Assert.Equal("pending", detail!.Status);
        Assert.Equal(item.UpdatedAt, detail.UpdatedAt);
    }

    [Fact]
    public async Task Revoke_of_a_missing_id_is_not_found()
    {
        factory.Reset();
        var res = await factory.Client().PostAsync("/api/work-items/999999/revoke", null);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }
}
