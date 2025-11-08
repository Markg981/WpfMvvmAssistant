namespace Core.Domain.Models;

public class QueryTab
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "New Query";
    public string QueryText { get; set; } = string.Empty;
    public string? ConnectionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public bool IsDirty { get; set; }
    public QueryResult? LastResult { get; set; }
}
