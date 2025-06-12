using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using ServerTest.Extension.DIContainer;
using ServerTest.Implement;
using ServerTest.Service;
using IServiceProvider = ServerTest.Extension.DIContainer.IServiceProvider;

namespace ServerTest
{
    public class Global : HttpApplication
    {
        private static IServiceProvider _rootServiceProvider;
        private const string ServiceScopeKey = "RequestServiceScope";

        private void ConfigureServices(IServiceCollection services)
        { 
            services.AddScoped<IContactService, ContactService>();
            _rootServiceProvider = services.BuildServiceProvider();
            Application["RootServiceProvider"] = _rootServiceProvider;
        }
        
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (_rootServiceProvider == null) return;
            var scopeFactory = _rootServiceProvider.GetRequiredService<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();
            HttpContext.Current.Items[ServiceScopeKey] = scope;
            HttpContext.Current.Items["ServiceProvider"] = scope.ServiceProvider;
        }
        
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.Items[ServiceScopeKey] is IDisposable scope)
                scope.Dispose();
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            ConfigureServices(new ServiceCollection());
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End(object sender, EventArgs e)
        {
            if (_rootServiceProvider is IDisposable disposableRootProvider)
                disposableRootProvider.Dispose();
        }
    }
}