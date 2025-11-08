using Core.Domain.Models;
using Core.DataAccess.Interfaces;
using Core.Services.Interfaces;
using Serilog;

namespace Core.Services.Services;

public class SchemaService : ISchemaService
{
    private readonly ISchemaReader _schemaReader;
    private readonly ILogger _logger;

    public SchemaService(ISchemaReader schemaReader, ILogger logger)
    {
        _schemaReader = schemaReader ?? throw new ArgumentNullException(nameof(schemaReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DatabaseSchema> GetSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Reading database schema...");
        return await _schemaReader.ReadSchemaAsync(connectionString, cancellationToken);
    }

    public async Task<List<string>> GetDatabasesAsync(
        string serverConnectionString,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Reading database list...");
        return await _schemaReader.GetDatabaseListAsync(serverConnectionString, cancellationToken);
    }

    public async Task<TableInfo> GetTableDetailsAsync(
        string connectionString,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Reading table details for {Schema}.{Table}", schemaName, tableName);
        return await _schemaReader.GetTableDetailsAsync(connectionString, schemaName, tableName, cancellationToken);
    }

    public string GenerateSelectQuery(TableInfo table, int topRows = 100)
    {
        _logger.Information("Generating SELECT query for {Schema}.{Table}", table.SchemaName, table.TableName);

        return $"SELECT TOP {topRows} *{Environment.NewLine}" +
               $"FROM [{table.SchemaName}].[{table.TableName}];";
    }
}
