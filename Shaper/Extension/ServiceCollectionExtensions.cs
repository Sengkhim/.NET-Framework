using Shaper.Core.DependencyInjection.Service;

namespace Shaper.Extension
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTransient<TService>(this IServiceCollection services)
            where TService : class
        {
            return services.AddTransient<TService, TService>();
        }

        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services)
            where TService : class
        {
            return services.AddSingleton<TService, TService>();
        }

        public static IServiceCollection AddScoped<TService>(this IServiceCollection services)
            where TService : class
        {
            return services.AddScoped<TService, TService>();
        }
    }
}