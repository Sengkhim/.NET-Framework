using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Shaper.Core.Connection.Service;
using Shaper.Core.DependencyInjection.Service;
using Shaper.Extension;

namespace Shaper.Core.Connection.Implement;

public class DbConnectionProvider(
    IConfiguration configuration) : IDbConnectionProvider, IDisposable
{
    private readonly ConcurrentDictionary<string, IDbConnection> _connections = new();

    public async Task<IDbConnection> GetConnection(string name)
    {
        SqlConnection connection = null;

        if (_connections.TryGetValue(name, out var con))
            return con;

        var connectionString = configuration.GetConnectionString(name);

        await ResiliencyPolicy.HandleRetry.ExecuteAsync(async () =>
        {
            connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
        });
        
        _connections[name] = connection;
        return connection;
    }

    public void Dispose()
    {
        foreach (var conn in _connections.Values)
            conn.Dispose();
        
        _connections.Clear();
    }
}