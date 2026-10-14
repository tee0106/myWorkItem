using System.Net.Http.Json;
using WorkItem.Api.Models;

namespace WorkItem.Api.Tests;

public sealed class ConfirmationPersistenceTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Name_A_stays_confirmed_and_name_B_stays_pending()
    {
        factory.Reset();
        var admin = factory.Client("Admin", "admin");
        var item = await ApiFactory.CreateItem(admin, "Shared");
        var a = factory.Client("A");
        await a.PostAsJsonAsync("/api/work-items/confirm", new { ids = new[] { item.Id } });
        var again = await factory.Client("A").GetFromJsonAsync<WorkItemDetail>($"/api/work-items/{item.Id}");
        var b = await factory.Client("B").GetFromJsonAsync<WorkItemDetail>($"/api/work-items/{item.Id}");
        Assert.Equal("confirmed", again!.Status);
        Assert.Equal("pending", b!.Status);
    }
}
