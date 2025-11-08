using Core.Domain.Models;

namespace Core.Services.Interfaces;

public interface IQueryService
{
    Task<QueryResult> ExecuteQueryAsync(
        QueryExecutionContext context,
        CancellationToken cancellationToken = default);

    Task<NaturalLanguageQueryResult> TranslateNaturalLanguageQueryAsync(
        NaturalLanguageQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<QueryResult> ExecuteNaturalLanguageQueryAsync(
        string connectionString,
        NaturalLanguageQueryRequest request,
        CancellationToken cancellationToken = default);
}
