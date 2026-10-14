using System.Net;
using System.Net.Http.Json;
using WorkItem.Api.Models;

namespace WorkItem.Api.Tests;

public sealed class WorkItemListEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Returns_an_empty_page()
    {
        factory.Reset();
        var page = await factory.Client().GetFromJsonAsync<WorkItemPage>("/api/work-items");
        Assert.NotNull(page);
        Assert.Empty(page.Items);
        Assert.Equal(0, page.Total);
        Assert.Equal(10, page.PageSize);
        Assert.Equal("desc", page.Direction);
    }

    [Fact]
    public async Task Defaults_to_newest_first_and_can_sort_oldest_first()
    {
        factory.Reset();
        var admin = factory.Client("Admin", "admin");
        var first = await ApiFactory.CreateItem(admin, "First");
        var second = await ApiFactory.CreateItem(admin, "Second");
        var desc = await factory.Client().GetFromJsonAsync<WorkItemPage>("/api/work-items");
        Assert.Equal(second.Id, desc!.Items[0].Id);
        Assert.Equal(first.Id, desc.Items[1].Id);
        var asc = await factory.Client().GetFromJsonAsync<WorkItemPage>("/api/work-items?direction=asc");
        Assert.Equal("asc", asc!.Direction);
        Assert.Equal(first.Id, asc.Items[0].Id);
    }

    [Fact]
    public async Task Pages_ten_items_and_a_page_past_the_end_is_empty()
    {
        factory.Reset();
        var admin = factory.Client("Admin", "admin");
        for (var i = 0; i < 11; i++)
            await ApiFactory.CreateItem(admin, $"Item {i}");
        var user = factory.Client();
        var first = await user.GetFromJsonAsync<WorkItemPage>("/api/work-items?page=1");
        var second = await user.GetFromJsonAsync<WorkItemPage>("/api/work-items?page=2");
        var past = await user.GetFromJsonAsync<WorkItemPage>("/api/work-items?page=3");
        Assert.Equal(10, first!.Items.Count);
        Assert.Equal(11, first.Total);
        Assert.Single(second!.Items);
        Assert.Empty(past!.Items);
        Assert.Equal(11, past.Total);
    }

    [Fact]
    public async Task Rejects_a_call_without_a_display_name()
    {
        var res = await factory.Client(name: null).GetAsync("/api/work-items");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        Assert.Equal("sign_in_required", (await res.Content.ReadFromJsonAsync<Error>())!.Code);
    }
}
