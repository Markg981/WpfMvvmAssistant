using System.Data;

namespace Core.Domain.Models;

public class QueryResult
{
    public bool Success { get; set; }
    public DataTable? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int RowsAffected { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public string? QueryText { get; set; }
}

public class QueryExecutionContext
{
    public string ConnectionString { get; set; } = string.Empty;
    public string QueryText { get; set; } = string.Empty;
    public QueryType QueryType { get; set; } = QueryType.Sql;
    public int Timeout { get; set; } = 30;
    public CancellationToken CancellationToken { get; set; }
}

public enum QueryType
{
    Sql,
    EntityFramework,
    NaturalLanguage
}
