using Core.Domain.Models;

namespace Core.Services.Interfaces;

public interface IConnectionService
{
    Task<List<ConnectionInfo>> GetConnectionsAsync();
    
    Task<ConnectionInfo?> GetConnectionByIdAsync(string id);
    
    Task SaveConnectionAsync(ConnectionInfo connection);
    
    Task DeleteConnectionAsync(string id);
    
    Task<bool> TestConnectionAsync(ConnectionInfo connection);
    
    Task UpdateLastUsedAsync(string id);
}
