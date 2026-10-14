using System.Text.Json.Serialization;

namespace WorkItem.Api.Models;

public static class Role
{
    public const string User = "user";
    public const string Admin = "admin";
}

public static class Status
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
}

public sealed record Session(string DisplayName, string Role);
public sealed record SignIn(string? DisplayName, string? Role);
public sealed record WorkItemSummary(long Id, string Title, string Status, string CreatedAt);
public sealed record WorkItemDetail(long Id, string Title, string Status, string CreatedAt, string Description, string UpdatedAt);
public sealed record WorkItemPage(int Page, int PageSize, int Total, string Direction, IReadOnlyList<WorkItemSummary> Items);
public sealed record AdminWorkItem(long Id, string Title, string Description, string CreatedAt, string UpdatedAt);
public sealed record AdminWorkItemPage(int Page, int PageSize, int Total, string Direction, IReadOnlyList<AdminWorkItem> Items);
public sealed record WorkItemWrite(string? Title, string? Description);
public sealed record ConfirmRequest(long[]? Ids);
public sealed record ConfirmResult(int ConfirmedCount);
public sealed record RevokeResult(int RevokedCount);
public sealed record Error([property: JsonPropertyName("error")] string Code);
public sealed record FieldError([property: JsonPropertyName("error")] string Code, [property: JsonPropertyName("field")] string Field);
public sealed record SaveOutcome(string? Error, AdminWorkItem? Item);
