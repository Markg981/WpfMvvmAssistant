using Core.Domain.Models;

namespace Core.DataAccess.Interfaces;

public interface IQueryExecutor
{
    Task<QueryResult> ExecuteSqlQueryAsync(
        string connectionString,
        string query,
        int timeout = 30,
        CancellationToken cancellationToken = default);

    Task<QueryResult> ExecuteEntityFrameworkQueryAsync(
        string connectionString,
        string linqQuery,
        CancellationToken cancellationToken = default);

    Task<bool> TestConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken = default);
}
