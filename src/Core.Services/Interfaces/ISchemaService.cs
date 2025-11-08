using Core.Domain.Models;

namespace Core.Services.Interfaces;

public interface ISchemaService
{
    Task<DatabaseSchema> GetSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetDatabasesAsync(
        string serverConnectionString,
        CancellationToken cancellationToken = default);

    Task<TableInfo> GetTableDetailsAsync(
        string connectionString,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken = default);

    string GenerateSelectQuery(TableInfo table, int topRows = 100);
}
