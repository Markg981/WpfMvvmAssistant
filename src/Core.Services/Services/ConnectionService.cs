using System.Text.Json;
using Core.Domain.Models;
using Core.DataAccess.Interfaces;
using Core.Services.Interfaces;
using Serilog;

namespace Core.Services.Services;

public class ConnectionService : IConnectionService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly ILogger _logger;
    private readonly string _connectionFilePath;

    public ConnectionService(IQueryExecutor queryExecutor, ILogger logger)
    {
        _queryExecutor = queryExecutor ?? throw new ArgumentNullException(nameof(queryExecutor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "SqlStudioPro");
        Directory.CreateDirectory(appFolder);
        _connectionFilePath = Path.Combine(appFolder, "connections.json");
    }

    public async Task<List<ConnectionInfo>> GetConnectionsAsync()
    {
        try
        {
            if (!File.Exists(_connectionFilePath))
            {
                _logger.Information("No connections file found, returning empty list");
                return new List<ConnectionInfo>();
            }

            var json = await File.ReadAllTextAsync(_connectionFilePath);
            var connections = JsonSerializer.Deserialize<List<ConnectionInfo>>(json) ?? new List<ConnectionInfo>();

            _logger.Information("Loaded {Count} connections", connections.Count);
            return connections;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load connections: {ErrorMessage}", ex.Message);
            return new List<ConnectionInfo>();
        }
    }

    public async Task<ConnectionInfo?> GetConnectionByIdAsync(string id)
    {
        var connections = await GetConnectionsAsync();
        return connections.FirstOrDefault(c => c.Id == id);
    }

    public async Task SaveConnectionAsync(ConnectionInfo connection)
    {
        try
        {
            var connections = await GetConnectionsAsync();

            var existing = connections.FirstOrDefault(c => c.Id == connection.Id);
            if (existing != null)
            {
                connections.Remove(existing);
                _logger.Information("Updating existing connection: {Name}", connection.Name);
            }
            else
            {
                _logger.Information("Adding new connection: {Name}", connection.Name);
            }

            connections.Add(connection);

            var json = JsonSerializer.Serialize(connections, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_connectionFilePath, json);

            _logger.Information("Connection saved successfully: {Name}", connection.Name);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save connection: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    public async Task DeleteConnectionAsync(string id)
    {
        try
        {
            var connections = await GetConnectionsAsync();
            var connection = connections.FirstOrDefault(c => c.Id == id);

            if (connection != null)
            {
                connections.Remove(connection);

                var json = JsonSerializer.Serialize(connections, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(_connectionFilePath, json);

                _logger.Information("Connection deleted: {Name}", connection.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete connection: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    public async Task<bool> TestConnectionAsync(ConnectionInfo connection)
    {
        try
        {
            _logger.Information("Testing connection to: {ServerName}/{DatabaseName}",
                connection.ServerName, connection.DatabaseName);

            var connectionString = connection.GetConnectionString();
            return await _queryExecutor.TestConnectionAsync(connectionString);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Connection test failed: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    public async Task UpdateLastUsedAsync(string id)
    {
        try
        {
            var connection = await GetConnectionByIdAsync(id);
            if (connection != null)
            {
                connection.LastUsed = DateTime.UtcNow;
                await SaveConnectionAsync(connection);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to update last used timestamp: {ErrorMessage}", ex.Message);
        }
    }
}
