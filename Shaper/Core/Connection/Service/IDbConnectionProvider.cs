using System.Data;
using System.Threading.Tasks;

namespace Shaper.Core.Connection.Service;

public interface IDbConnectionProvider
{
    Task<IDbConnection> GetConnection(string name);
}