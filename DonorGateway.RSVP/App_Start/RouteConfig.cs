using System.Web.Mvc;
using System.Web.Routing;

namespace DonorGateway.RSVP
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("RegisterAction", "Register", new { controller = "Event", action = "Register" });

            routes.MapRoute("FinishAction", "Finish", new { controller = "Event", action = "Finish" });

            routes.MapRoute("ConfirmAction", "Confirm", new { controller = "Event", action = "Confirm" });

            routes.MapRoute("EventAction", "{id}", new { controller = "Event", action = "Index", id = UrlParameter.Optional });

            routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Event", action = "Index", id = UrlParameter.Optional });
        }
    }
}
