using System.Web;
using Shaper.Core.Connection.Implement;
using Shaper.Core.Connection.Service;
using Shaper.Core.DependencyInjection.Implement;
using Shaper.Core.DependencyInjection.Service;
using Web3.Implement;
using Web3.Service;

namespace Web3.Extension;

public static class ServiceCollectionExtension
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        var configPath = HttpContext.Current.Server.MapPath("~/App_Data/Config");
        services.AddSingleton<IConfiguration>(new JsonConfiguration(configPath));

        services.AddScoped<IDbConnectionProvider, DbConnectionProvider>();
        services.AddScoped<IContactService, ContactService>();
    } 
}