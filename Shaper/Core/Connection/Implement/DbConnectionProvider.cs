using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using Shaper.Core.Connection.Service;
using Shaper.Core.DependencyInjection.Service;

namespace Shaper.Core.Connection.Implement;

public class DbConnectionProvider(IConfiguration configuration) : IDbConnectionProvider, IDisposable
{
    private readonly ConcurrentDictionary<string, IDbConnection> _connections = new();

    public IDbConnection GetConnection(string name = "Connection")
    {
        if (_connections.TryGetValue(name, out var con))
            return con;

        var connectionString = configuration.GetConnectionString(name);
        var connection = new SqlConnection(connectionString);
        connection.Open();
        
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