using System.Net;
using System.Net.Http.Json;
using WorkItem.Api.Models;

namespace WorkItem.Api.Tests;

public sealed class WorkItemDetailEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Returns_the_item_fields_and_the_caller_status()
    {
        factory.Reset();
        var admin = factory.Client("Admin", "admin");
        var created = await ApiFactory.CreateItem(admin, "Hello", "Body");
        var user = factory.Client();
        await user.PostAsJsonAsync("/api/work-items/confirm", new { ids = new[] { created.Id } });
        var detail = await user.GetFromJsonAsync<WorkItemDetail>($"/api/work-items/{created.Id}");
        Assert.Equal(created.Id, detail!.Id);
        Assert.Equal("Hello", detail.Title);
        Assert.Equal("Body", detail.Description);
        Assert.Equal(created.CreatedAt, detail.CreatedAt);
        Assert.Equal(created.UpdatedAt, detail.UpdatedAt);
        Assert.Equal("confirmed", detail.Status);
    }

    [Fact]
    public async Task Missing_id_is_not_found()
    {
        factory.Reset();
        var res = await factory.Client().GetAsync("/api/work-items/999999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal("not_found", (await res.Content.ReadFromJsonAsync<Error>())!.Code);
    }
}
