using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Shaper.Core.Connection.Service;
using Web3.Infrastructure.Model;

namespace Web3.Infrastructure.Repository;

public interface IProductionRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
}


public class ProductionRepository(IDbConnectionProvider provider)
    : IProductionRepository
{
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var context = await provider.GetConnection("Connection");
        
        var result =  await context
            .QueryAsync<Product>("SELECT * FROM dbo.Products");
        
        return result;
    }
}
