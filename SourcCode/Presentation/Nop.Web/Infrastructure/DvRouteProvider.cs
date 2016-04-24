using System;
using System.Web.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routes;
using Nop.Web.Framework.Seo;

namespace Nop.Web.Infrastructure
{
    public class DvRouteProvider : IRouteProvider
    {
        public int Priority
        {
            get
            {
                //it should be the last route
                //we do not set it to -int.MaxValue so it could be overridden (if required)
                return 0;
            }
        }

        public void RegisterRoutes(RouteCollection routes)
        {
            // reviews
            routes.MapLocalizedRoute("CustomerReviews",
                            "customerreviews",
                            new { controller = "Product", action = "CustomerReviews" },
                            new[] { "Nop.Web.Controllers" });
        }
    }
}