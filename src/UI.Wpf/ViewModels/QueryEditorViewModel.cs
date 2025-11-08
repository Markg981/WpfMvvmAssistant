using System.Data;
using System.Windows.Input;
using Core.Domain.Models;
using Core.Services.Interfaces;
using UI.Wpf.Commands;
using Serilog;

namespace UI.Wpf.ViewModels;

public class QueryEditorViewModel : ViewModelBase
{
    private readonly IQueryService _queryService;
    private readonly ISchemaService _schemaService;
    private readonly ILogger _logger;

    private string _queryText = string.Empty;
    private string _naturalLanguageText = string.Empty;
    private string _generatedSql = string.Empty;
    private string _generatedLinq = string.Empty;
    private string _translationExplanation = string.Empty;
    private double _confidence;
    private DataTable? _results;
    private string _statusMessage = "Ready";
    private bool _isExecuting;
    private string? _connectionString;
    private DatabaseSchema? _schema;

    public QueryEditorViewModel(IQueryService queryService, ISchemaService schemaService, ILogger logger)
    {
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        _schemaService = schemaService ?? throw new ArgumentNullException(nameof(schemaService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ExecuteSqlCommand = new AsyncRelayCommand(async _ => await ExecuteSqlAsync(), _ => CanExecute());
        ExecuteEfCommand = new AsyncRelayCommand(async _ => await ExecuteEfAsync(), _ => CanExecute());
        TranslateNlCommand = new AsyncRelayCommand(async _ => await TranslateNaturalLanguageAsync(), _ => CanTranslate());
        ExecuteNlCommand = new AsyncRelayCommand(async _ => await ExecuteNaturalLanguageAsync(), _ => CanExecute());
        ClearCommand = new RelayCommand(_ => Clear());
    }

    public string QueryText
    {
        get => _queryText;
        set => SetProperty(ref _queryText, value);
    }

    public string NaturalLanguageText
    {
        get => _naturalLanguageText;
        set => SetProperty(ref _naturalLanguageText, value);
    }

    public string GeneratedSql
    {
        get => _generatedSql;
        set => SetProperty(ref _generatedSql, value);
    }

    public string GeneratedLinq
    {
        get => _generatedLinq;
        set => SetProperty(ref _generatedLinq, value);
    }

    public string TranslationExplanation
    {
        get => _translationExplanation;
        set => SetProperty(ref _translationExplanation, value);
    }

    public double Confidence
    {
        get => _confidence;
        set => SetProperty(ref _confidence, value);
    }

    public DataTable? Results
    {
        get => _results;
        set => SetProperty(ref _results, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsExecuting
    {
        get => _isExecuting;
        set => SetProperty(ref _isExecuting, value);
    }

    public string? ConnectionString
    {
        get => _connectionString;
        set => SetProperty(ref _connectionString, value);
    }

    public DatabaseSchema? Schema
    {
        get => _schema;
        set => SetProperty(ref _schema, value);
    }

    public ICommand ExecuteSqlCommand { get; }
    public ICommand ExecuteEfCommand { get; }
    public ICommand TranslateNlCommand { get; }
    public ICommand ExecuteNlCommand { get; }
    public ICommand ClearCommand { get; }

    private bool CanExecute() => !string.IsNullOrWhiteSpace(ConnectionString) && !IsExecuting;

    private bool CanTranslate() => !string.IsNullOrWhiteSpace(NaturalLanguageText) && Schema != null && !IsExecuting;

    private async Task ExecuteSqlAsync()
    {
        if (string.IsNullOrWhiteSpace(QueryText) || string.IsNullOrWhiteSpace(ConnectionString))
            return;

        try
        {
            IsExecuting = true;
            StatusMessage = "Executing SQL query...";

            var context = new QueryExecutionContext
            {
                ConnectionString = ConnectionString,
                QueryText = QueryText,
                QueryType = QueryType.Sql
            };

            var result = await _queryService.ExecuteQueryAsync(context);

            if (result.Success)
            {
                Results = result.Data;
                StatusMessage = $"Query executed successfully. {result.RowsAffected} rows affected. ({result.ExecutionTime.TotalMilliseconds:F2}ms)";
            }
            else
            {
                Results = null;
                StatusMessage = $"Error: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "SQL execution failed");
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsExecuting = false;
        }
    }

    private async Task ExecuteEfAsync()
    {
        try
        {
            IsExecuting = true;
            StatusMessage = "Executing Entity Framework query...";

            await Task.Delay(500);

            StatusMessage = "Entity Framework execution is not yet implemented. Use SQL queries instead.";
        }
        finally
        {
            IsExecuting = false;
        }
    }

    private async Task TranslateNaturalLanguageAsync()
    {
        if (string.IsNullOrWhiteSpace(NaturalLanguageText) || Schema == null)
            return;

        try
        {
            IsExecuting = true;
            StatusMessage = "Translating natural language query...";

            var request = new NaturalLanguageQueryRequest
            {
                NaturalLanguageText = NaturalLanguageText,
                Schema = Schema
            };

            var result = await _queryService.TranslateNaturalLanguageQueryAsync(request);

            if (result.Success)
            {
                GeneratedSql = result.SqlQuery;
                GeneratedLinq = result.EntityFrameworkQuery;
                TranslationExplanation = result.Explanation;
                Confidence = result.Confidence;
                QueryText = result.SqlQuery;

                StatusMessage = $"Translation successful. Confidence: {result.Confidence:P0}";
            }
            else
            {
                StatusMessage = $"Translation failed: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Natural language translation failed");
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsExecuting = false;
        }
    }

    private async Task ExecuteNaturalLanguageAsync()
    {
        await TranslateNaturalLanguageAsync();

        if (!string.IsNullOrWhiteSpace(GeneratedSql))
        {
            await ExecuteSqlAsync();
        }
    }

    private void Clear()
    {
        QueryText = string.Empty;
        Results = null;
        GeneratedSql = string.Empty;
        GeneratedLinq = string.Empty;
        TranslationExplanation = string.Empty;
        StatusMessage = "Ready";
    }
}
