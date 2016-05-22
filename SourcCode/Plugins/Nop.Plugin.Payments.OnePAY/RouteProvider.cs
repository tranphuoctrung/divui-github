using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.OnePAY
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.OnePAY.Configure",
                 "Plugins/PaymentOnePAY/Configure",
                 new { controller = "PaymentOnePAY", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.OnePAY.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.OnePAY.PaymentInfo",
                 "Plugins/PaymentOnePAY/PaymentInfo",
                 new { controller = "PaymentOnePAY", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.OnePAY.Controllers" }
            );
            
            //Result payment
            routes.MapRoute("Plugin.Payments.OnePAY.PDTHandler",
                 "Plugins/PaymentOnePAY/PDTHandler",
                 new { controller = "PaymentOnePAY", action = "PDTHandler" },
                 new[] { "Nop.Plugin.Payments.OnePAY.Controllers" }
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
