using Core.Domain.Models;
using Core.DataAccess.Interfaces;
using Core.Nlp.Interfaces;
using Core.Services.Interfaces;
using Serilog;

namespace Core.Services.Services;

public class QueryService : IQueryService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly INaturalLanguageQueryTranslator _nlpTranslator;
    private readonly ILogger _logger;

    public QueryService(
        IQueryExecutor queryExecutor,
        INaturalLanguageQueryTranslator nlpTranslator,
        ILogger logger)
    {
        _queryExecutor = queryExecutor ?? throw new ArgumentNullException(nameof(queryExecutor));
        _nlpTranslator = nlpTranslator ?? throw new ArgumentNullException(nameof(nlpTranslator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QueryResult> ExecuteQueryAsync(
        QueryExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Executing query of type: {QueryType}", context.QueryType);

        return context.QueryType switch
        {
            QueryType.Sql => await _queryExecutor.ExecuteSqlQueryAsync(
                context.ConnectionString,
                context.QueryText,
                context.Timeout,
                cancellationToken),

            QueryType.EntityFramework => await _queryExecutor.ExecuteEntityFrameworkQueryAsync(
                context.ConnectionString,
                context.QueryText,
                cancellationToken),

            _ => throw new ArgumentException($"Unsupported query type: {context.QueryType}")
        };
    }

    public async Task<NaturalLanguageQueryResult> TranslateNaturalLanguageQueryAsync(
        NaturalLanguageQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Translating natural language query: {Query}", request.NaturalLanguageText);

        if (!_nlpTranslator.IsAvailable())
        {
            return new NaturalLanguageQueryResult
            {
                Success = false,
                ErrorMessage = "Natural language translator is not available"
            };
        }

        return await _nlpTranslator.TranslateAsync(request, cancellationToken);
    }

    public async Task<QueryResult> ExecuteNaturalLanguageQueryAsync(
        string connectionString,
        NaturalLanguageQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Executing natural language query: {Query}", request.NaturalLanguageText);

        var translationResult = await TranslateNaturalLanguageQueryAsync(request, cancellationToken);

        if (!translationResult.Success)
        {
            return new QueryResult
            {
                Success = false,
                ErrorMessage = $"Translation failed: {translationResult.ErrorMessage}",
                QueryText = request.NaturalLanguageText
            };
        }

        return await _queryExecutor.ExecuteSqlQueryAsync(
            connectionString,
            translationResult.SqlQuery,
            30,
            cancellationToken);
    }
}
