using System.Data;
using System.Threading.Tasks;

namespace ShaperUtilities.Core.Connection.Service;

public interface IDbConnectionProvider
{
    Task<IDbConnection> GetConnection(string name);
}