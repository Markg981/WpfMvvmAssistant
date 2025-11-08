namespace Core.Domain.Models;

public class DatabaseSchema
{
    public string ServerName { get; set; } = string.Empty;
    public List<DatabaseInfo> Databases { get; set; } = new();
}

public class DatabaseInfo
{
    public string Name { get; set; } = string.Empty;
    public List<SchemaInfo> Schemas { get; set; } = new();
}

public class SchemaInfo
{
    public string Name { get; set; } = string.Empty;
    public List<TableInfo> Tables { get; set; } = new();
    public List<ViewInfo> Views { get; set; } = new();
    public List<StoredProcedureInfo> StoredProcedures { get; set; } = new();
    public List<FunctionInfo> Functions { get; set; } = new();
}

public class TableInfo
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string FullName => $"[{SchemaName}].[{TableName}]";
    public List<ColumnInfo> Columns { get; set; } = new();
    public List<IndexInfo> Indexes { get; set; } = new();
    public List<ForeignKeyInfo> ForeignKeys { get; set; } = new();
}

public class ColumnInfo
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
    public bool IsIdentity { get; set; }
    public string? DefaultValue { get; set; }
}

public class IndexInfo
{
    public string IndexName { get; set; } = string.Empty;
    public bool IsUnique { get; set; }
    public bool IsPrimaryKey { get; set; }
    public List<string> Columns { get; set; } = new();
}

public class ForeignKeyInfo
{
    public string ForeignKeyName { get; set; } = string.Empty;
    public string ReferencedTable { get; set; } = string.Empty;
    public string ReferencedSchema { get; set; } = string.Empty;
    public Dictionary<string, string> ColumnMapping { get; set; } = new();
}

public class ViewInfo
{
    public string SchemaName { get; set; } = string.Empty;
    public string ViewName { get; set; } = string.Empty;
    public string FullName => $"[{SchemaName}].[{ViewName}]";
    public string? Definition { get; set; }
}

public class StoredProcedureInfo
{
    public string SchemaName { get; set; } = string.Empty;
    public string ProcedureName { get; set; } = string.Empty;
    public string FullName => $"[{SchemaName}].[{ProcedureName}]";
    public List<ParameterInfo> Parameters { get; set; } = new();
}

public class FunctionInfo
{
    public string SchemaName { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public string FullName => $"[{SchemaName}].[{FunctionName}]";
    public string FunctionType { get; set; } = string.Empty;
    public List<ParameterInfo> Parameters { get; set; } = new();
}

public class ParameterInfo
{
    public string ParameterName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public bool IsOutput { get; set; }
}
