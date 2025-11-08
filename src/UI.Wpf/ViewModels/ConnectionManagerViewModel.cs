using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.Domain.Models;
using Core.Services.Interfaces;
using UI.Wpf.Commands;
using Serilog;

namespace UI.Wpf.ViewModels;

public class ConnectionManagerViewModel : ViewModelBase
{
    private readonly IConnectionService _connectionService;
    private readonly ILogger _logger;

    private ConnectionInfo? _selectedConnection;
    private string _serverName = string.Empty;
    private string _databaseName = string.Empty;
    private string _connectionName = string.Empty;
    private bool _useWindowsAuth = true;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isTestingConnection;

    public ConnectionManagerViewModel(IConnectionService connectionService, ILogger logger)
    {
        _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Connections = new ObservableCollection<ConnectionInfo>();

        NewCommand = new RelayCommand(_ => CreateNewConnection());
        SaveCommand = new AsyncRelayCommand(async _ => await SaveConnectionAsync(), _ => CanSave());
        DeleteCommand = new AsyncRelayCommand(async _ => await DeleteConnectionAsync(), _ => SelectedConnection != null);
        TestConnectionCommand = new AsyncRelayCommand(async _ => await TestConnectionAsync(), _ => CanTestConnection());
        ConnectCommand = new RelayCommand(_ => Connect(), _ => SelectedConnection != null);

        _ = LoadConnectionsAsync();
    }

    public event Action<bool> RequestClose;

    private void OnRequestClose(bool dialogResult)
    {
        RequestClose?.Invoke(dialogResult);
    }

    public ObservableCollection<ConnectionInfo> Connections { get; }

    public ConnectionInfo? SelectedConnection
    {
        get => _selectedConnection;
        set
        {
            if (SetProperty(ref _selectedConnection, value))
            {
                LoadConnectionDetails();
            }
        }
    }

    public string ServerName
    {
        get => _serverName;
        set => SetProperty(ref _serverName, value);
    }

    public string DatabaseName
    {
        get => _databaseName;
        set => SetProperty(ref _databaseName, value);
    }

    public string ConnectionName
    {
        get => _connectionName;
        set => SetProperty(ref _connectionName, value);
    }

    public bool UseWindowsAuth
    {
        get => _useWindowsAuth;
        set => SetProperty(ref _useWindowsAuth, value);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsTestingConnection
    {
        get => _isTestingConnection;
        set => SetProperty(ref _isTestingConnection, value);
    }

    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand TestConnectionCommand { get; }
    public ICommand ConnectCommand { get; }

    private void Connect()
    {
        OnRequestClose(true);
    }

    private async Task LoadConnectionsAsync()
    {
        try
        {
            var connections = await _connectionService.GetConnectionsAsync();
            Connections.Clear();

            foreach (var connection in connections)
            {
                Connections.Add(connection);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load connections");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void CreateNewConnection()
    {
        SelectedConnection = null;
        ServerName = string.Empty;
        DatabaseName = string.Empty;
        ConnectionName = string.Empty;
        UseWindowsAuth = true;
        Username = string.Empty;
        Password = string.Empty;
        StatusMessage = "Enter new connection details";
    }

    private void LoadConnectionDetails()
    {
        if (SelectedConnection == null)
        {
            CreateNewConnection();
            return;
        }

        ServerName = SelectedConnection.ServerName;
        DatabaseName = SelectedConnection.DatabaseName;
        ConnectionName = SelectedConnection.Name;
        UseWindowsAuth = SelectedConnection.UseWindowsAuth;
        Username = SelectedConnection.Username ?? string.Empty;
        Password = string.Empty;
        StatusMessage = "Connection loaded";
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(ServerName) &&
               !string.IsNullOrWhiteSpace(DatabaseName) &&
               !string.IsNullOrWhiteSpace(ConnectionName);
    }

    private bool CanTestConnection()
    {
        return CanSave() && !IsTestingConnection;
    }

    private async Task SaveConnectionAsync()
    {
        try
        {
            var connection = SelectedConnection ?? new ConnectionInfo();

            connection.Name = ConnectionName;
            connection.ServerName = ServerName;
            connection.DatabaseName = DatabaseName;
            connection.UseWindowsAuth = UseWindowsAuth;
            connection.Username = UseWindowsAuth ? null : Username;

            if (!UseWindowsAuth && !string.IsNullOrEmpty(Password))
            {
                connection.SetPassword(Password);
            }

            await _connectionService.SaveConnectionAsync(connection);

            await LoadConnectionsAsync();

            SelectedConnection = Connections.FirstOrDefault(c => c.Id == connection.Id);

            StatusMessage = "Connection saved successfully";
            _logger.Information("Connection saved: {ConnectionName}", connection.Name);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save connection");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task DeleteConnectionAsync()
    {
        if (SelectedConnection == null)
            return;

        try
        {
            await _connectionService.DeleteConnectionAsync(SelectedConnection.Id);
            await LoadConnectionsAsync();
            CreateNewConnection();
            StatusMessage = "Connection deleted";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete connection");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task TestConnectionAsync()
    {
        try
        {
            IsTestingConnection = true;
            StatusMessage = "Testing connection...";

            var testConnection = new ConnectionInfo
            {
                ServerName = ServerName,
                DatabaseName = DatabaseName,
                UseWindowsAuth = UseWindowsAuth,
                Username = UseWindowsAuth ? null : Username
            };

            if (!UseWindowsAuth && !string.IsNullOrEmpty(Password))
            {
                testConnection.SetPassword(Password);
            }

            var success = await _connectionService.TestConnectionAsync(testConnection);

            StatusMessage = success ? "✓ Connection successful!" : "✗ Connection failed";
            _logger.Information("Connection test result: {Success}", success);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Connection test error");
            StatusMessage = $"✗ Error: {ex.Message}";
        }
        finally
        {
            IsTestingConnection = false;
        }
    }
}
