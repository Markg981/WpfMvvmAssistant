using System.Text;
using System.Text.RegularExpressions;
using Core.Domain.Models;
using Core.Nlp.Interfaces;
using Serilog;

namespace Core.Nlp.Implementations;

public class RuleBasedNaturalLanguageTranslator : INaturalLanguageQueryTranslator
{
    private readonly ILogger _logger;

    public RuleBasedNaturalLanguageTranslator(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NaturalLanguageQueryResult> TranslateAsync(
        NaturalLanguageQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Translating natural language query: {Query}", request.NaturalLanguageText);

        var result = new NaturalLanguageQueryResult();

        try
        {
            var parser = new QueryParser(request.NaturalLanguageText, request.Schema, request.DefaultSchema ?? "dbo");
            var parsedQuery = parser.Parse();

            if (!parsedQuery.IsValid)
            {
                result.Success = false;
                result.ErrorMessage = parsedQuery.ErrorMessage;
                result.Confidence = 0.0;
                return result;
            }

            result.IdentifiedTables = parsedQuery.Tables;
            result.IdentifiedColumns = parsedQuery.Columns;

            result.SqlQuery = GenerateSqlQuery(parsedQuery);
            result.EntityFrameworkQuery = GenerateEntityFrameworkQuery(parsedQuery);
            result.Explanation = GenerateExplanation(parsedQuery);
            result.Confidence = CalculateConfidence(parsedQuery);
            result.Warnings = parsedQuery.Warnings;
            result.Success = true;

            _logger.Information("Translation successful. Confidence: {Confidence:P}", result.Confidence);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Translation failed: {ErrorMessage}", ex.Message);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Confidence = 0.0;
        }

        await Task.CompletedTask;
        return result;
    }

    public bool IsAvailable() => true;

    public string GetProviderName() => "Rule-Based NLP Translator (Mock)";

    private string GenerateSqlQuery(ParsedQuery query)
    {
        var sql = new StringBuilder();

        sql.Append("SELECT ");

        if (query.Limit.HasValue)
            sql.Append($"TOP {query.Limit.Value} ");

        if (query.Columns.Count > 0)
            sql.Append(string.Join(", ", query.Columns));
        else
            sql.Append("*");

        sql.AppendLine();
        sql.Append($"FROM {query.Tables[0]}");

        if (query.Conditions.Count > 0)
        {
            sql.AppendLine();
            sql.Append("WHERE ");
            sql.Append(string.Join(" AND ", query.Conditions.Select(c => FormatConditionForSql(c))));
        }

        if (query.OrderByColumns.Count > 0)
        {
            sql.AppendLine();
            sql.Append("ORDER BY ");
            sql.Append(string.Join(", ", query.OrderByColumns.Select(c => 
                $"{c.Column} {(c.Descending ? "DESC" : "ASC")}")));
        }

        return sql.ToString();
    }

    private string GenerateEntityFrameworkQuery(ParsedQuery query)
    {
        var ef = new StringBuilder();
        var tableName = query.Tables[0].Split('.').Last().Trim('[', ']');

        ef.AppendLine($"var query = dbContext.{MakePlural(tableName)}");

        if (query.Conditions.Count > 0)
        {
            var conditions = query.Conditions.Select(c => FormatConditionForLinq(c));
            ef.AppendLine($"    .Where({string.Join(" && ", conditions)})");
        }

        if (query.OrderByColumns.Count > 0)
        {
            var firstOrder = query.OrderByColumns[0];
            var orderMethod = firstOrder.Descending ? "OrderByDescending" : "OrderBy";
            ef.AppendLine($"    .{orderMethod}(x => x.{firstOrder.Column})");

            for (int i = 1; i < query.OrderByColumns.Count; i++)
            {
                var order = query.OrderByColumns[i];
                var thenMethod = order.Descending ? "ThenByDescending" : "ThenBy";
                ef.AppendLine($"    .{thenMethod}(x => x.{order.Column})");
            }
        }

        if (query.Limit.HasValue)
        {
            ef.AppendLine($"    .Take({query.Limit.Value})");
        }

        ef.Append("    .ToListAsync();");

        return ef.ToString();
    }

    private string GenerateExplanation(ParsedQuery query)
    {
        var explanation = new StringBuilder();

        explanation.Append($"Query targets table: {query.Tables[0]}. ");

        if (query.Conditions.Count > 0)
        {
            explanation.Append($"Filtering by {query.Conditions.Count} condition(s): ");
            explanation.Append(string.Join(", ", query.Conditions.Select(c => $"{c.Column} {c.Operator} {c.Value}")));
            explanation.Append(". ");
        }

        if (query.OrderByColumns.Count > 0)
        {
            explanation.Append($"Ordered by {string.Join(", ", query.OrderByColumns.Select(c => c.Column))}. ");
        }

        if (query.Limit.HasValue)
        {
            explanation.Append($"Limited to {query.Limit.Value} rows.");
        }

        return explanation.ToString();
    }

    private double CalculateConfidence(ParsedQuery query)
    {
        double confidence = 0.5;

        if (query.Tables.Count > 0) confidence += 0.2;
        if (query.Conditions.Count > 0) confidence += 0.15;
        if (query.Columns.Count > 0) confidence += 0.1;
        if (query.Warnings.Count == 0) confidence += 0.05;

        return Math.Min(confidence, 1.0);
    }

    private string FormatConditionForSql(QueryCondition condition)
    {
        if (condition.UseParameter)
        {
            return $"{condition.Column} {condition.Operator} '{condition.Value}'";
        }
        return $"{condition.Column} {condition.Operator} {condition.Value}";
    }

    private string FormatConditionForLinq(QueryCondition condition)
    {
        var op = condition.Operator switch
        {
            "=" => "==",
            "!=" => "!=",
            ">" => ">",
            "<" => "<",
            ">=" => ">=",
            "<=" => "<=",
            "LIKE" => ".Contains",
            _ => "=="
        };

        if (op == ".Contains")
        {
            var value = condition.Value.Trim('\'', '%');
            return $"x.{condition.Column}.Contains(\"{value}\")";
        }

        var linqValue = condition.Value;
        if (condition.UseParameter && !linqValue.StartsWith("\""))
        {
            linqValue = $"\"{linqValue.Trim('\'')}\"";
        }

        return $"x.{condition.Column} {op} {linqValue}";
    }

    private string MakePlural(string word)
    {
        if (word.EndsWith("y"))
            return word.Substring(0, word.Length - 1) + "ies";
        if (word.EndsWith("s"))
            return word + "es";
        return word + "s";
    }

    private class QueryParser
    {
        // Pre-compiled static regex patterns for performance optimization
        private static readonly Regex LastDaysRegex = new(@"last\s+(\d+)\s+days", RegexOptions.Compiled);
        private static readonly Regex TopRegex = new(@"top\s+(\d+)", RegexOptions.Compiled);
        private static readonly Regex FirstRegex = new(@"first\s+(\d+)", RegexOptions.Compiled);

        private readonly string _input;
        private readonly DatabaseSchema _schema;
        private readonly string _defaultSchema;

        public QueryParser(string input, DatabaseSchema schema, string defaultSchema)
        {
            _input = input.ToLower();
            _schema = schema;
            _defaultSchema = defaultSchema;
        }

        public ParsedQuery Parse()
        {
            var query = new ParsedQuery();

            var tables = FindTables();
            if (tables.Count == 0)
            {
                query.IsValid = false;
                query.ErrorMessage = "Could not identify any tables in the query. Please specify a table name.";
                return query;
            }

            query.Tables = tables.Select(t => $"[{t.SchemaName}].[{t.TableName}]").ToList();

            var mainTable = tables[0];
            query.Columns = FindColumns(mainTable);

            query.Conditions = FindConditions(mainTable);

            query.Limit = FindLimit();

            query.OrderByColumns = FindOrderBy(mainTable);

            query.IsValid = true;
            return query;
        }

        private List<TableInfo> FindTables()
        {
            var foundTables = new List<TableInfo>();

            foreach (var db in _schema.Databases)
            {
                foreach (var schema in db.Schemas)
                {
                    foreach (var table in schema.Tables)
                    {
                        var tableName = table.TableName.ToLower();
                        var pluralName = MakePlural(tableName);

                        if (_input.Contains(tableName) || _input.Contains(pluralName))
                        {
                            foundTables.Add(table);
                        }
                    }
                }
            }

            return foundTables;
        }

        private List<string> FindColumns(TableInfo table)
        {
            var columns = new List<string>();

            foreach (var column in table.Columns)
            {
                if (_input.Contains(column.ColumnName.ToLower()))
                {
                    columns.Add(column.ColumnName);
                }
            }

            return columns;
        }

        private List<QueryCondition> FindConditions(TableInfo table)
        {
            var conditions = new List<QueryCondition>();

            foreach (var column in table.Columns)
            {
                var columnName = column.ColumnName.ToLower();

                if (column.DataType.Contains("bit") && _input.Contains("active"))
                {
                    if (columnName.Contains("active"))
                    {
                        conditions.Add(new QueryCondition
                        {
                            Column = column.ColumnName,
                            Operator = "=",
                            Value = "1",
                            UseParameter = false
                        });
                    }
                }

                if (columnName.Contains("date"))
                {
                    if (_input.Contains("last") && _input.Contains("days"))
                    {
                        var daysMatch = LastDaysRegex.Match(_input);
                        if (daysMatch.Success)
                        {
                            var days = daysMatch.Groups[1].Value;
                            conditions.Add(new QueryCondition
                            {
                                Column = column.ColumnName,
                                Operator = ">=",
                                Value = $"DATEADD(day, -{days}, GETDATE())",
                                UseParameter = false
                            });
                        }
                    }
                    else if (_input.Contains("yesterday"))
                    {
                        conditions.Add(new QueryCondition
                        {
                            Column = column.ColumnName,
                            Operator = ">=",
                            Value = "CAST(GETDATE() - 1 AS DATE)",
                            UseParameter = false
                        });
                    }
                    else if (_input.Contains("today"))
                    {
                        conditions.Add(new QueryCondition
                        {
                            Column = column.ColumnName,
                            Operator = ">=",
                            Value = "CAST(GETDATE() AS DATE)",
                            UseParameter = false
                        });
                    }
                }

                if (column.DataType.Contains("varchar") || column.DataType.Contains("char"))
                {
                    var pattern = $@"where\s+{columnName}\s+(is|=|equals?)\s+['""]?(\w+)['""]?";
                    var match = Regex.Match(_input, pattern);
                    if (match.Success)
                    {
                        conditions.Add(new QueryCondition
                        {
                            Column = column.ColumnName,
                            Operator = "=",
                            Value = match.Groups[2].Value,
                            UseParameter = true
                        });
                    }
                }

                if (column.DataType.Contains("int") || column.DataType.Contains("decimal") || column.DataType.Contains("money"))
                {
                    var greaterPattern = $@"{columnName}\s+(greater|more)\s+than\s+(\d+)";
                    var greaterMatch = Regex.Match(_input, greaterPattern);
                    if (greaterMatch.Success)
                    {
                        conditions.Add(new QueryCondition
                        {
                            Column = column.ColumnName,
                            Operator = ">",
                            Value = greaterMatch.Groups[2].Value,
                            UseParameter = false
                        });
                    }

                    var lessPattern = $@"{columnName}\s+(less|fewer)\s+than\s+(\d+)";
                    var lessMatch = Regex.Match(_input, lessPattern);
                    if (lessMatch.Success)
                    {
                        conditions.Add(new QueryCondition
                        {
                            Column = column.ColumnName,
                            Operator = "<",
                            Value = lessMatch.Groups[2].Value,
                            UseParameter = false
                        });
                    }
                }
            }

            return conditions;
        }

        private int? FindLimit()
        {
            var topMatch = TopRegex.Match(_input);
            if (topMatch.Success)
            {
                return int.Parse(topMatch.Groups[1].Value);
            }

            var firstMatch = FirstRegex.Match(_input);
            if (firstMatch.Success)
            {
                return int.Parse(firstMatch.Groups[1].Value);
            }

            return null;
        }

        private List<OrderByColumn> FindOrderBy(TableInfo table)
        {
            var orderColumns = new List<OrderByColumn>();

            foreach (var column in table.Columns)
            {
                var columnName = column.ColumnName.ToLower();

                if (_input.Contains($"order by {columnName}") || 
                    _input.Contains($"sorted by {columnName}") ||
                    _input.Contains($"sort by {columnName}"))
                {
                    orderColumns.Add(new OrderByColumn
                    {
                        Column = column.ColumnName,
                        Descending = _input.Contains("desc") || _input.Contains("descending")
                    });
                }
            }

            return orderColumns;
        }

        private string MakePlural(string word)
        {
            if (word.EndsWith("y"))
                return word.Substring(0, word.Length - 1) + "ies";
            if (word.EndsWith("s"))
                return word + "es";
            return word + "s";
        }
    }

    private class ParsedQuery
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Tables { get; set; } = new();
        public List<string> Columns { get; set; } = new();
        public List<QueryCondition> Conditions { get; set; } = new();
        public int? Limit { get; set; }
        public List<OrderByColumn> OrderByColumns { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    private class QueryCondition
    {
        public string Column { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool UseParameter { get; set; }
    }

    private class OrderByColumn
    {
        public string Column { get; set; } = string.Empty;
        public bool Descending { get; set; }
    }
}
