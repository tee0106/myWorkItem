using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.Sqlite;
using WorkItem.Api.Models;

namespace WorkItem.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public string DbPath { get; } = Path.Combine(Path.GetTempPath(), $"workitems-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Path"] = DbPath,
            });
        });
    }

    public HttpClient Client(string? name = "A", string role = "user")
    {
        var client = CreateClient();
        if (name is not null)
        {
            client.DefaultRequestHeaders.Add("X-Display-Name", name);
            client.DefaultRequestHeaders.Add("X-Role", role);
        }
        return client;
    }

    public void Reset()
    {
        using var start = CreateClient();
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM confirmation; DELETE FROM work_item;";
        cmd.ExecuteNonQuery();
    }

    public long Count(string sql)
    {
        using var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return (long)cmd.ExecuteScalar()!;
    }

    public static async Task<AdminWorkItem> CreateItem(HttpClient admin, string title, string? description = "")
    {
        var res = await admin.PostAsJsonAsync("/api/admin/work-items", new { title, description });
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<AdminWorkItem>())!;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        using (var conn = new SqliteConnection($"Data Source={DbPath}"))
        {
            conn.Open();
            SqliteConnection.ClearPool(conn);
        }
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var file = DbPath + suffix;
            if (File.Exists(file)) File.Delete(file);
        }
    }
}
