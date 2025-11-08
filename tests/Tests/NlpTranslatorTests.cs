using Core.Domain.Models;
using Core.Nlp.Implementations;
using Serilog;
using Xunit;

namespace Tests;

public class NlpTranslatorTests
{
    private readonly RuleBasedNaturalLanguageTranslator _translator;
    private readonly DatabaseSchema _testSchema;

    public NlpTranslatorTests()
    {
        var logger = new LoggerConfiguration()
            .WriteTo.Debug()
            .CreateLogger();

        _translator = new RuleBasedNaturalLanguageTranslator(logger);

        _testSchema = new DatabaseSchema
        {
            ServerName = "TestServer",
            Databases = new List<DatabaseInfo>
            {
                new DatabaseInfo
                {
                    Name = "TestDB",
                    Schemas = new List<SchemaInfo>
                    {
                        new SchemaInfo
                        {
                            Name = "dbo",
                            Tables = new List<TableInfo>
                            {
                                new TableInfo
                                {
                                    SchemaName = "dbo",
                                    TableName = "Customers",
                                    Columns = new List<ColumnInfo>
                                    {
                                        new ColumnInfo { ColumnName = "Id", DataType = "int", IsPrimaryKey = true },
                                        new ColumnInfo { ColumnName = "Name", DataType = "nvarchar" },
                                        new ColumnInfo { ColumnName = "Email", DataType = "nvarchar" },
                                        new ColumnInfo { ColumnName = "IsActive", DataType = "bit" },
                                        new ColumnInfo { ColumnName = "CreatedAt", DataType = "datetime2" }
                                    }
                                },
                                new TableInfo
                                {
                                    SchemaName = "dbo",
                                    TableName = "Orders",
                                    Columns = new List<ColumnInfo>
                                    {
                                        new ColumnInfo { ColumnName = "Id", DataType = "int", IsPrimaryKey = true },
                                        new ColumnInfo { ColumnName = "CustomerId", DataType = "int", IsForeignKey = true },
                                        new ColumnInfo { ColumnName = "OrderDate", DataType = "datetime2" },
                                        new ColumnInfo { ColumnName = "TotalAmount", DataType = "decimal" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    [Fact]
    public async Task TranslateAsync_SimpleQuery_ReturnsValidSql()
    {
        var request = new NaturalLanguageQueryRequest
        {
            NaturalLanguageText = "Show me all customers",
            Schema = _testSchema
        };

        var result = await _translator.TranslateAsync(request);

        Assert.True(result.Success);
        Assert.Contains("SELECT", result.SqlQuery);
        Assert.Contains("[dbo].[Customers]", result.SqlQuery);
        Assert.NotEmpty(result.EntityFrameworkQuery);
        Assert.True(result.Confidence > 0);
    }

    [Fact]
    public async Task TranslateAsync_ActiveCustomers_AddsWhereClause()
    {
        var request = new NaturalLanguageQueryRequest
        {
            NaturalLanguageText = "Get all active customers",
            Schema = _testSchema
        };

        var result = await _translator.TranslateAsync(request);

        Assert.True(result.Success);
        Assert.Contains("WHERE", result.SqlQuery);
        Assert.Contains("IsActive", result.SqlQuery);
    }

    [Fact]
    public async Task TranslateAsync_TopN_AddsLimit()
    {
        var request = new NaturalLanguageQueryRequest
        {
            NaturalLanguageText = "Show me top 10 customers",
            Schema = _testSchema
        };

        var result = await _translator.TranslateAsync(request);

        Assert.True(result.Success);
        Assert.Contains("TOP 10", result.SqlQuery);
        Assert.Contains(".Take(10)", result.EntityFrameworkQuery);
    }

    [Fact]
    public async Task TranslateAsync_NoMatchingTable_ReturnsError()
    {
        var request = new NaturalLanguageQueryRequest
        {
            NaturalLanguageText = "Show me all products",
            Schema = _testSchema
        };

        var result = await _translator.TranslateAsync(request);

        Assert.False(result.Success);
        Assert.Contains("Could not identify", result.ErrorMessage);
    }

    [Fact]
    public async Task TranslateAsync_DateRange_AddsDateCondition()
    {
        var request = new NaturalLanguageQueryRequest
        {
            NaturalLanguageText = "Show orders from the last 30 days",
            Schema = _testSchema
        };

        var result = await _translator.TranslateAsync(request);

        Assert.True(result.Success);
        Assert.Contains("OrderDate", result.SqlQuery);
        Assert.Contains("DATEADD", result.SqlQuery);
        Assert.Contains("30", result.SqlQuery);
    }

    [Fact]
    public void IsAvailable_ReturnsTrue()
    {
        Assert.True(_translator.IsAvailable());
    }

    [Fact]
    public void GetProviderName_ReturnsCorrectName()
    {
        var name = _translator.GetProviderName();
        Assert.Contains("Rule-Based", name);
    }
}
