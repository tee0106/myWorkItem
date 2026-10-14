using System.Net;
using System.Net.Http.Json;
using WorkItem.Api.Models;

namespace WorkItem.Api.Tests;

public sealed class SessionEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Trims_the_display_name()
    {
        var res = await factory.CreateClient().PostAsJsonAsync("/api/session", new { displayName = "  Ada  ", role = "user" });
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<Session>();
        Assert.Equal("Ada", body!.DisplayName);
        Assert.Equal("user", body.Role);
    }

    [Fact]
    public async Task Rejects_a_name_that_is_empty_after_trim()
    {
        var res = await factory.CreateClient().PostAsJsonAsync("/api/session", new { displayName = "   ", role = "user" });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("name_required", (await res.Content.ReadFromJsonAsync<Error>())!.Code);
    }

    [Fact]
    public async Task Rejects_a_role_other_than_user_or_admin()
    {
        var res = await factory.CreateClient().PostAsJsonAsync("/api/session", new { displayName = "Ada", role = "guest" });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("invalid_role", (await res.Content.ReadFromJsonAsync<Error>())!.Code);
    }
}
