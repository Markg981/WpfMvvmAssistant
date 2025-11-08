using Core.Domain.Models;

namespace Core.DataAccess.Interfaces;

public interface ISchemaReader
{
    Task<DatabaseSchema> ReadSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetDatabaseListAsync(
        string serverConnectionString,
        CancellationToken cancellationToken = default);

    Task<TableInfo> GetTableDetailsAsync(
        string connectionString,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken = default);
}
