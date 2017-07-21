using System.Web.Mvc;
using System.Web.Routing;

namespace Fanex.ActivityTracking.IntegrationTests.Site
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Demo", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}