namespace Core.Domain.Models;

public class NaturalLanguageQueryResult
{
    public string SqlQuery { get; set; } = string.Empty;
    public string EntityFrameworkQuery { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public List<string> IdentifiedTables { get; set; } = new();
    public List<string> IdentifiedColumns { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class NaturalLanguageQueryRequest
{
    public string NaturalLanguageText { get; set; } = string.Empty;
    public DatabaseSchema Schema { get; set; } = new();
    public string? TargetDatabase { get; set; }
    public string? DefaultSchema { get; set; } = "dbo";
}
