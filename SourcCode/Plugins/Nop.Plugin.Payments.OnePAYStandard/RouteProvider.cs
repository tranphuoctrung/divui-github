using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.OnePAY
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.OnePAYStandard.Configure",
                 "Plugins/PaymentOnePAYStandard/Configure",
                 new { controller = "PaymentOnePAYStandard", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.OnePAYStandard.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.OnePAYStandard.PaymentInfo",
                 "Plugins/PaymentOnePAYStandard/PaymentInfo",
                 new { controller = "PaymentOnePAYStandard", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.OnePAYStandard.Controllers" }
            );
            
            //Result payment
            routes.MapRoute("Plugin.Payments.OnePAYStandard.PDTHandler",
                 "Plugins/PaymentOnePAYStandard/PDTHandler",
                 new { controller = "PaymentOnePAYStandard", action = "PDTHandler" },
                 new[] { "Nop.Plugin.Payments.OnePAYStandard.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
