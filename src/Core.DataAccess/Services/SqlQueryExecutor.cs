using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Core.Domain.Models;
using Core.DataAccess.Interfaces;
using Serilog;

namespace Core.DataAccess.Services;

public class SqlQueryExecutor : IQueryExecutor
{
    private readonly ILogger _logger;

    public SqlQueryExecutor(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueryResult> ExecuteSqlQueryAsync(
        string connectionString,
        string query,
        int timeout = 30,
        CancellationToken cancellationToken = default)
    {
        var result = new QueryResult
        {
            QueryText = query
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.Information("Executing SQL query: {Query}", query.Length > 100 ? query.Substring(0, 100) + "..." : query);

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(query, connection)
            {
                CommandTimeout = timeout,
                CommandType = CommandType.Text
            };

            if (IsSelectQuery(query))
            {
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                var dataTable = new DataTable();
                dataTable.Load(reader);

                result.Data = dataTable;
                result.RowsAffected = dataTable.Rows.Count;
            }
            else
            {
                result.RowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            }

            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
            result.Success = true;

            _logger.Information("Query executed successfully in {ElapsedMs}ms. Rows affected: {RowsAffected}",
                stopwatch.ElapsedMilliseconds, result.RowsAffected);
        }
        catch (SqlException ex)
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
            result.Success = false;
            result.ErrorMessage = $"SQL Error {ex.Number}: {ex.Message}";

            _logger.Error(ex, "SQL query execution failed: {ErrorMessage}", ex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
            result.Success = false;
            result.ErrorMessage = ex.Message;

            _logger.Error(ex, "Query execution failed: {ErrorMessage}", ex.Message);
        }

        return result;
    }

    public async Task<QueryResult> ExecuteEntityFrameworkQueryAsync(
        string connectionString,
        string linqQuery,
        CancellationToken cancellationToken = default)
    {
        var result = new QueryResult
        {
            QueryText = linqQuery,
            Success = false,
            ErrorMessage = "Entity Framework query execution requires runtime compilation. " +
                          "This feature is planned for future releases. " +
                          "For now, please use SQL queries or convert LINQ to SQL manually."
        };

        _logger.Warning("EF LINQ execution attempted but not yet implemented: {LinqQuery}", linqQuery);

        await Task.CompletedTask;
        return result;
    }

    public async Task<bool> TestConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Testing database connection...");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            _logger.Information("Connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Connection test failed: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    private static bool IsSelectQuery(string query)
    {
        var trimmed = query.TrimStart();
        return trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("WITH", StringComparison.OrdinalIgnoreCase);
    }
}
