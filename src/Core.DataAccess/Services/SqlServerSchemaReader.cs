using Microsoft.Data.SqlClient;
using Core.Domain.Models;
using Core.DataAccess.Interfaces;
using Serilog;

namespace Core.DataAccess.Services;

public class SqlServerSchemaReader : ISchemaReader
{
    private readonly ILogger _logger;

    public SqlServerSchemaReader(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DatabaseSchema> ReadSchemaAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        var schema = new DatabaseSchema();

        try
        {
            _logger.Information("Reading database schema...");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            schema.ServerName = connection.DataSource;

            var dbInfo = new DatabaseInfo
            {
                Name = connection.Database,
                Schemas = await ReadSchemasAsync(connection, cancellationToken)
            };

            schema.Databases.Add(dbInfo);

            _logger.Information("Schema read successfully. Found {SchemaCount} schemas", dbInfo.Schemas.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read database schema: {ErrorMessage}", ex.Message);
            throw;
        }

        return schema;
    }

    public async Task<List<string>> GetDatabaseListAsync(
        string serverConnectionString,
        CancellationToken cancellationToken = default)
    {
        var databases = new List<string>();

        try
        {
            await using var connection = new SqlConnection(serverConnectionString);
            await connection.OpenAsync(cancellationToken);

            const string query = @"
                SELECT name 
                FROM sys.databases 
                WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')
                ORDER BY name";

            await using var command = new SqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                databases.Add(reader.GetString(0));
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get database list: {ErrorMessage}", ex.Message);
            throw;
        }

        return databases;
    }

    public async Task<TableInfo> GetTableDetailsAsync(
        string connectionString,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        var tableInfo = new TableInfo
        {
            SchemaName = schemaName,
            TableName = tableName
        };

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            tableInfo.Columns = await ReadColumnsAsync(connection, schemaName, tableName, cancellationToken);
            tableInfo.Indexes = await ReadIndexesAsync(connection, schemaName, tableName, cancellationToken);
            tableInfo.ForeignKeys = await ReadForeignKeysAsync(connection, schemaName, tableName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read table details for {Schema}.{Table}: {ErrorMessage}",
                schemaName, tableName, ex.Message);
            throw;
        }

        return tableInfo;
    }

    private async Task<List<SchemaInfo>> ReadSchemasAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        var schemas = new List<SchemaInfo>();

        const string query = "SELECT DISTINCT schema_name FROM INFORMATION_SCHEMA.SCHEMATA ORDER BY schema_name";

        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var schemaNames = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            schemaNames.Add(reader.GetString(0));
        }

        await reader.CloseAsync();

        foreach (var schemaName in schemaNames)
        {
            var schemaInfo = new SchemaInfo
            {
                Name = schemaName,
                Tables = await ReadTablesAsync(connection, schemaName, cancellationToken),
                Views = await ReadViewsAsync(connection, schemaName, cancellationToken),
                StoredProcedures = await ReadStoredProceduresAsync(connection, schemaName, cancellationToken),
                Functions = await ReadFunctionsAsync(connection, schemaName, cancellationToken)
            };

            schemas.Add(schemaInfo);
        }

        return schemas;
    }

    private async Task<List<TableInfo>> ReadTablesAsync(
        SqlConnection connection,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var tables = new List<TableInfo>();

        const string query = @"
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = @SchemaName
            AND TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_NAME";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var tableName = reader.GetString(0);
            tables.Add(new TableInfo
            {
                SchemaName = schemaName,
                TableName = tableName
            });
        }

        return tables;
    }

    private async Task<List<ColumnInfo>> ReadColumnsAsync(
        SqlConnection connection,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken)
    {
        var columns = new List<ColumnInfo>();

        const string query = @"
            SELECT 
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.CHARACTER_MAXIMUM_LENGTH,
                c.NUMERIC_PRECISION,
                c.NUMERIC_SCALE,
                c.IS_NULLABLE,
                c.COLUMN_DEFAULT,
                CAST(CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IS_PRIMARY_KEY,
                CAST(COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS BIT) AS IS_IDENTITY
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN (
                SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                    ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
            ) pk ON c.TABLE_SCHEMA = pk.TABLE_SCHEMA 
                AND c.TABLE_NAME = pk.TABLE_NAME 
                AND c.COLUMN_NAME = pk.COLUMN_NAME
            WHERE c.TABLE_SCHEMA = @SchemaName
            AND c.TABLE_NAME = @TableName
            ORDER BY c.ORDINAL_POSITION";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);
        command.Parameters.AddWithValue("@TableName", tableName);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(new ColumnInfo
            {
                ColumnName = reader.GetString(0),
                DataType = reader.GetString(1),
                MaxLength = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                Precision = reader.IsDBNull(3) ? null : Convert.ToInt32(reader.GetByte(3)),
                Scale = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                IsNullable = reader.GetString(5) == "YES",
                DefaultValue = reader.IsDBNull(6) ? null : reader.GetString(6),
                IsPrimaryKey = reader.GetBoolean(7),
                IsIdentity = reader.GetBoolean(8)
            });
        }

        return columns;
    }

    private async Task<List<IndexInfo>> ReadIndexesAsync(
        SqlConnection connection,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken)
    {
        var indexes = new List<IndexInfo>();

        const string query = @"
            SELECT 
                i.name AS IndexName,
                i.is_unique AS IsUnique,
                i.is_primary_key AS IsPrimaryKey,
                c.name AS ColumnName
            FROM sys.indexes i
            INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
            INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            INNER JOIN sys.tables t ON i.object_id = t.object_id
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = @SchemaName
            AND t.name = @TableName
            ORDER BY i.name, ic.key_ordinal";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);
        command.Parameters.AddWithValue("@TableName", tableName);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var indexDict = new Dictionary<string, IndexInfo>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var indexName = reader.GetString(0);
            var columnName = reader.GetString(3);

            if (!indexDict.TryGetValue(indexName, out var indexInfo))
            {
                indexInfo = new IndexInfo
                {
                    IndexName = indexName,
                    IsUnique = reader.GetBoolean(1),
                    IsPrimaryKey = reader.GetBoolean(2)
                };
                indexDict[indexName] = indexInfo;
                indexes.Add(indexInfo);
            }

            indexInfo.Columns.Add(columnName);
        }

        return indexes;
    }

    private async Task<List<ForeignKeyInfo>> ReadForeignKeysAsync(
        SqlConnection connection,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken)
    {
        var foreignKeys = new List<ForeignKeyInfo>();

        const string query = @"
            SELECT 
                fk.name AS ForeignKeyName,
                SCHEMA_NAME(t2.schema_id) AS ReferencedSchema,
                t2.name AS ReferencedTable,
                c1.name AS ColumnName,
                c2.name AS ReferencedColumnName
            FROM sys.foreign_keys fk
            INNER JOIN sys.tables t1 ON fk.parent_object_id = t1.object_id
            INNER JOIN sys.tables t2 ON fk.referenced_object_id = t2.object_id
            INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            INNER JOIN sys.columns c1 ON fkc.parent_object_id = c1.object_id AND fkc.parent_column_id = c1.column_id
            INNER JOIN sys.columns c2 ON fkc.referenced_object_id = c2.object_id AND fkc.referenced_column_id = c2.column_id
            WHERE SCHEMA_NAME(t1.schema_id) = @SchemaName
            AND t1.name = @TableName";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);
        command.Parameters.AddWithValue("@TableName", tableName);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var fkDict = new Dictionary<string, ForeignKeyInfo>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var fkName = reader.GetString(0);
            var columnName = reader.GetString(3);
            var referencedColumnName = reader.GetString(4);

            if (!fkDict.TryGetValue(fkName, out var fkInfo))
            {
                fkInfo = new ForeignKeyInfo
                {
                    ForeignKeyName = fkName,
                    ReferencedSchema = reader.GetString(1),
                    ReferencedTable = reader.GetString(2)
                };
                fkDict[fkName] = fkInfo;
                foreignKeys.Add(fkInfo);
            }

            fkInfo.ColumnMapping[columnName] = referencedColumnName;
        }

        return foreignKeys;
    }

    private async Task<List<ViewInfo>> ReadViewsAsync(
        SqlConnection connection,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var views = new List<ViewInfo>();

        const string query = @"
            SELECT TABLE_NAME, VIEW_DEFINITION
            FROM INFORMATION_SCHEMA.VIEWS
            WHERE TABLE_SCHEMA = @SchemaName
            ORDER BY TABLE_NAME";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            views.Add(new ViewInfo
            {
                SchemaName = schemaName,
                ViewName = reader.GetString(0),
                Definition = reader.IsDBNull(1) ? null : reader.GetString(1)
            });
        }

        return views;
    }

    private async Task<List<StoredProcedureInfo>> ReadStoredProceduresAsync(
        SqlConnection connection,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var procedures = new List<StoredProcedureInfo>();

        const string query = @"
            SELECT ROUTINE_NAME
            FROM INFORMATION_SCHEMA.ROUTINES
            WHERE ROUTINE_SCHEMA = @SchemaName
            AND ROUTINE_TYPE = 'PROCEDURE'
            ORDER BY ROUTINE_NAME";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            procedures.Add(new StoredProcedureInfo
            {
                SchemaName = schemaName,
                ProcedureName = reader.GetString(0)
            });
        }

        return procedures;
    }

    private async Task<List<FunctionInfo>> ReadFunctionsAsync(
        SqlConnection connection,
        string schemaName,
        CancellationToken cancellationToken)
    {
        var functions = new List<FunctionInfo>();

        const string query = @"
            SELECT ROUTINE_NAME, DATA_TYPE
            FROM INFORMATION_SCHEMA.ROUTINES
            WHERE ROUTINE_SCHEMA = @SchemaName
            AND ROUTINE_TYPE = 'FUNCTION'
            ORDER BY ROUTINE_NAME";

        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            functions.Add(new FunctionInfo
            {
                SchemaName = schemaName,
                FunctionName = reader.GetString(0),
                FunctionType = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1)
            });
        }

        return functions;
    }
}
