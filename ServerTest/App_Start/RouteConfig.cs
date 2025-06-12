using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;

namespace ServerTest
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            var settings = new FriendlyUrlSettings
            {
                AutoRedirectMode = RedirectMode.Permanent,
                ResolverCachingMode = ResolverCachingMode.Dynamic
            };
            routes.EnableFriendlyUrls(settings);
        }
    }
}
