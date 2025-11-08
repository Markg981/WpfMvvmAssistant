using Core.Domain.Models;
using Core.Services.Services;
using Core.DataAccess.Interfaces;
using Moq;
using Serilog;
using Xunit;

namespace Tests;

public class ConnectionServiceTests
{
    private readonly Mock<IQueryExecutor> _mockQueryExecutor;
    private readonly ConnectionService _connectionService;

    public ConnectionServiceTests()
    {
        _mockQueryExecutor = new Mock<IQueryExecutor>();
        
        var logger = new LoggerConfiguration()
            .WriteTo.Debug()
            .CreateLogger();

        _connectionService = new ConnectionService(_mockQueryExecutor.Object, logger);
    }

    [Fact]
    public async Task SaveConnectionAsync_NewConnection_AddsToList()
    {
        var connection = new ConnectionInfo
        {
            Name = "Test Connection",
            ServerName = "localhost",
            DatabaseName = "TestDB",
            UseWindowsAuth = true
        };

        await _connectionService.SaveConnectionAsync(connection);

        var connections = await _connectionService.GetConnectionsAsync();
        Assert.Contains(connections, c => c.Id == connection.Id);
    }

    [Fact]
    public async Task GetConnectionByIdAsync_ExistingConnection_ReturnsConnection()
    {
        var connection = new ConnectionInfo
        {
            Name = "Test Connection",
            ServerName = "localhost",
            DatabaseName = "TestDB"
        };

        await _connectionService.SaveConnectionAsync(connection);
        var retrieved = await _connectionService.GetConnectionByIdAsync(connection.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(connection.Id, retrieved.Id);
        Assert.Equal(connection.Name, retrieved.Name);
    }

    [Fact]
    public async Task TestConnectionAsync_ValidConnection_ReturnsTrue()
    {
        _mockQueryExecutor
            .Setup(x => x.TestConnectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var connection = new ConnectionInfo
        {
            ServerName = "localhost",
            DatabaseName = "TestDB",
            UseWindowsAuth = true
        };

        var result = await _connectionService.TestConnectionAsync(connection);

        Assert.True(result);
    }

    [Fact]
    public async Task TestConnectionAsync_InvalidConnection_ReturnsFalse()
    {
        _mockQueryExecutor
            .Setup(x => x.TestConnectionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var connection = new ConnectionInfo
        {
            ServerName = "invalid",
            DatabaseName = "TestDB"
        };

        var result = await _connectionService.TestConnectionAsync(connection);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteConnectionAsync_ExistingConnection_RemovesFromList()
    {
        var connection = new ConnectionInfo
        {
            Name = "Test Connection",
            ServerName = "localhost",
            DatabaseName = "TestDB"
        };

        await _connectionService.SaveConnectionAsync(connection);
        await _connectionService.DeleteConnectionAsync(connection.Id);

        var retrieved = await _connectionService.GetConnectionByIdAsync(connection.Id);
        Assert.Null(retrieved);
    }
}
