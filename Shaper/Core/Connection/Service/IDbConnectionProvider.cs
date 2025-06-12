using System.Data;

namespace Shaper.Core.Connection.Service;

public interface IDbConnectionProvider
{
    IDbConnection GetConnection(string name);
}