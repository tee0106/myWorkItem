using System.Globalization;
using Dapper;
using WorkItem.Api.Models;

namespace WorkItem.Api.Data;

public sealed class WorkItemRepository(SqliteDatabase db)
{
    public const int PageSize = 10;

    public async Task<WorkItemPage> ListAsync(string displayName, string? direction, int page)
    {
        var dir = Dir(direction);
        page = Math.Max(page, 1);
        using var conn = db.Open();
        var total = await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM work_item");
        var offset = (page - 1) * PageSize;
        if (offset >= total)
            return new WorkItemPage(page, PageSize, (int)total, dir, []);
        var sql = $"""
            SELECT w.id AS Id, w.title AS Title,
                   CAST(CASE WHEN c.work_item_id IS NULL THEN 'pending' ELSE 'confirmed' END AS TEXT) AS Status,
                   w.created_at AS CreatedAt
            FROM work_item w
            LEFT JOIN confirmation c ON c.work_item_id = w.id AND c.display_name = @Name
            ORDER BY {Order(dir)}
            LIMIT @Limit OFFSET @Offset
            """;
        var items = (await conn.QueryAsync<WorkItemSummary>(sql, new { Name = displayName, Limit = PageSize, Offset = offset })).ToList();
        return new WorkItemPage(page, PageSize, (int)total, dir, items);
    }

    public async Task<WorkItemDetail?> GetAsync(string displayName, long id)
    {
        using var conn = db.Open();
        return await conn.QuerySingleOrDefaultAsync<WorkItemDetail>("""
            SELECT w.id AS Id, w.title AS Title,
                   CAST(CASE WHEN c.work_item_id IS NULL THEN 'pending' ELSE 'confirmed' END AS TEXT) AS Status,
                   w.created_at AS CreatedAt, w.description AS Description, w.updated_at AS UpdatedAt
            FROM work_item w
            LEFT JOIN confirmation c ON c.work_item_id = w.id AND c.display_name = @Name
            WHERE w.id = @Id
            """, new { Name = displayName, Id = id });
    }

    public async Task<int?> ConfirmAsync(string displayName, IReadOnlyList<long> ids)
    {
        var distinct = ids.Distinct().ToArray();
        using var conn = db.Open();
        using var tx = conn.BeginTransaction();
        var found = (await conn.QueryAsync<long>("SELECT id FROM work_item WHERE id IN @Ids", new { Ids = distinct }, tx)).ToArray();
        if (found.Length != distinct.Length)
        {
            tx.Rollback();
            return null;
        }
        await conn.ExecuteAsync(
            "INSERT OR IGNORE INTO confirmation (display_name, work_item_id) VALUES (@Name, @Id)",
            distinct.Select(id => new { Name = displayName, Id = id }),
            tx);
        tx.Commit();
        return distinct.Length;
    }

    public async Task<bool> RevokeAsync(string displayName, long id)
    {
        using var conn = db.Open();
        var exists = await conn.ExecuteScalarAsync<long?>("SELECT id FROM work_item WHERE id = @Id", new { Id = id });
        if (exists is null) return false;
        await conn.ExecuteAsync(
            "DELETE FROM confirmation WHERE display_name = @Name AND work_item_id = @Id",
            new { Name = displayName, Id = id });
        return true;
    }

    public async Task<AdminWorkItemPage> ListAdminAsync(string? direction, int page)
    {
        var dir = Dir(direction);
        page = Math.Max(page, 1);
        using var conn = db.Open();
        var total = await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM work_item");
        var offset = (page - 1) * PageSize;
        if (offset >= total)
            return new AdminWorkItemPage(page, PageSize, (int)total, dir, []);
        var items = (await conn.QueryAsync<AdminWorkItem>($"""
            SELECT id AS Id, title AS Title, description AS Description, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM work_item
            ORDER BY {Order(dir)}
            LIMIT @Limit OFFSET @Offset
            """, new { Limit = PageSize, Offset = offset })).ToList();
        return new AdminWorkItemPage(page, PageSize, (int)total, dir, items);
    }

    public async Task<AdminWorkItem?> GetAdminAsync(long id)
    {
        using var conn = db.Open();
        return await conn.QuerySingleOrDefaultAsync<AdminWorkItem>("""
            SELECT id AS Id, title AS Title, description AS Description, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM work_item WHERE id = @Id
            """, new { Id = id });
    }

    public async Task<SaveOutcome> CreateAsync(string? title, string? description)
    {
        var clean = title?.Trim() ?? "";
        if (clean.Length == 0) return new SaveOutcome("validation", null);
        var now = Stamp();
        using var conn = db.Open();
        await conn.ExecuteAsync(
            "INSERT INTO work_item (title, description, created_at, updated_at) VALUES (@Title, @Description, @Now, @Now)",
            new { Title = clean, Description = description?.Trim() ?? "", Now = now });
        var id = await conn.ExecuteScalarAsync<long>("SELECT last_insert_rowid()");
        return new SaveOutcome(null, await GetAdminAsync(conn, id));
    }

    public async Task<SaveOutcome> UpdateAsync(long id, string? title, string? description)
    {
        var clean = title?.Trim() ?? "";
        if (clean.Length == 0) return new SaveOutcome("validation", null);
        using var conn = db.Open();
        var n = await conn.ExecuteAsync(
            "UPDATE work_item SET title = @Title, description = @Description, updated_at = @Now WHERE id = @Id",
            new { Title = clean, Description = description?.Trim() ?? "", Now = Stamp(), Id = id });
        if (n == 0) return new SaveOutcome("not_found", null);
        return new SaveOutcome(null, await GetAdminAsync(conn, id));
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var conn = db.Open();
        return await conn.ExecuteAsync("DELETE FROM work_item WHERE id = @Id", new { Id = id }) > 0;
    }

    private static async Task<AdminWorkItem> GetAdminAsync(Microsoft.Data.Sqlite.SqliteConnection conn, long id) =>
        await conn.QuerySingleAsync<AdminWorkItem>("""
            SELECT id AS Id, title AS Title, description AS Description, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM work_item WHERE id = @Id
            """, new { Id = id });

    private static string Dir(string? direction) => direction == "asc" ? "asc" : "desc";

    private static string Order(string direction) =>
        direction == "asc" ? "created_at ASC, id ASC" : "created_at DESC, id DESC";

    private static string Stamp() =>
        DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
}
