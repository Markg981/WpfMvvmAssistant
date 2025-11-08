using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.Domain.Models;
using Core.Services.Interfaces;
using UI.Wpf.Commands;
using Serilog;

namespace UI.Wpf.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IConnectionService _connectionService;
    private readonly ISchemaService _schemaService;
    private readonly ILogger _logger;

    private ConnectionInfo? _selectedConnection;
    private DatabaseSchema? _currentSchema;
    private bool _isDarkTheme;
    private string _statusMessage = "Ready";

    public MainWindowViewModel(
        IConnectionService connectionService,
        ISchemaService schemaService,
        ILogger logger)
    {
        _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
        _schemaService = schemaService ?? throw new ArgumentNullException(nameof(schemaService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        QueryTabs = new ObservableCollection<QueryTab>();

        NewConnectionCommand = new AsyncRelayCommand(async _ => await ShowConnectionManagerAsync());
        NewQueryCommand = new RelayCommand(_ => CreateNewQueryTab());
        ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
        ExitCommand = new RelayCommand(_ => System.Windows.Application.Current.Shutdown());

        _ = LoadInitialDataAsync();
    }

    public ObservableCollection<QueryTab> QueryTabs { get; }

    public ConnectionInfo? SelectedConnection
    {
        get => _selectedConnection;
        set
        {
            if (SetProperty(ref _selectedConnection, value))
            {
                _ = LoadSchemaAsync();
            }
        }
    }

    public DatabaseSchema? CurrentSchema
    {
        get => _currentSchema;
        set => SetProperty(ref _currentSchema, value);
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set => SetProperty(ref _isDarkTheme, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand NewConnectionCommand { get; }
    public ICommand NewQueryCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand ExitCommand { get; }

    private async Task LoadInitialDataAsync()
    {
        try
        {
            _logger.Information("Loading initial data...");
            StatusMessage = "Loading connections...";

            var connections = await _connectionService.GetConnectionsAsync();
            if (connections.Any())
            {
                SelectedConnection = connections.OrderByDescending(c => c.LastUsed).FirstOrDefault();
            }

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load initial data");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task LoadSchemaAsync()
    {
        if (SelectedConnection == null)
            return;

        try
        {
            _logger.Information("Loading schema for connection: {ConnectionName}", SelectedConnection.Name);
            StatusMessage = $"Loading schema for {SelectedConnection.Name}...";

            var connectionString = SelectedConnection.GetConnectionString();
            CurrentSchema = await _schemaService.GetSchemaAsync(connectionString);

            await _connectionService.UpdateLastUsedAsync(SelectedConnection.Id);

            StatusMessage = $"Connected to {SelectedConnection.Name}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load schema");
            StatusMessage = $"Error loading schema: {ex.Message}";
            CurrentSchema = null;
        }
    }

    private async Task ShowConnectionManagerAsync()
    {
        StatusMessage = "Opening connection manager...";
        
        var connectionManagerWindow = new Views.ConnectionManagerWindow();
        var viewModel = new ConnectionManagerViewModel(_connectionService, _logger);
        connectionManagerWindow.DataContext = viewModel;

        if (connectionManagerWindow.ShowDialog() == true)
        {
            var newConnection = viewModel.SelectedConnection;
            if (newConnection != null)
            {
                // If the selected connection is the same as the current one, the property setter won't trigger a refresh.
                // We must manually trigger the schema load in that case.
                if (_selectedConnection?.Id == newConnection.Id)
                {
                    await LoadSchemaAsync();
                }
                else
                {
                    SelectedConnection = newConnection;
                }
            }
        }
        else
        {
            // If the user closed the dialog without connecting, reload the initial data
            // to ensure the UI is consistent.
            await LoadInitialDataAsync();
        }
    }

    private void CreateNewQueryTab()
    {
        var newTab = new QueryTab
        {
            Title = $"Query {QueryTabs.Count + 1}",
            ConnectionId = SelectedConnection?.Id
        };

        QueryTabs.Add(newTab);
        _logger.Information("Created new query tab: {TabId}", newTab.Id);
        StatusMessage = "New query tab created";
    }

    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        StatusMessage = $"Theme switched to {(IsDarkTheme ? "Dark" : "Light")}";
    }
}
