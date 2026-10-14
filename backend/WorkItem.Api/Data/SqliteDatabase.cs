using Dapper;
using Microsoft.Data.Sqlite;

namespace WorkItem.Api.Data;

public sealed class SqliteDatabase
{
    public string Path { get; }

    public SqliteDatabase(IConfiguration config, IWebHostEnvironment env)
    {
        var configured = config["Database:Path"];
        Path = string.IsNullOrWhiteSpace(configured)
            ? System.IO.Path.GetFullPath(System.IO.Path.Combine(env.ContentRootPath, "..", "data", "workitems.db"))
            : System.IO.Path.IsPathRooted(configured)
                ? configured
                : System.IO.Path.GetFullPath(System.IO.Path.Combine(env.ContentRootPath, configured));
    }

    public void Initialize()
    {
        var directory = System.IO.Path.GetDirectoryName(Path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        var schemaPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(SqliteDatabase).Assembly.Location)!, "Data", "schema.sql");
        using var conn = Open();
        conn.Execute(File.ReadAllText(schemaPath));
    }

    public SqliteConnection Open()
    {
        var conn = new SqliteConnection($"Data Source={Path}");
        conn.Open();
        conn.Execute("PRAGMA foreign_keys = ON;");
        return conn;
    }
}
