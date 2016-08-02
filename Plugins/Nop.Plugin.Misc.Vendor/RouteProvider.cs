using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Misc.Vendor
{
    public partial class RouteProvider : IRouteProvider
    {
		public void RegisterRoutes(RouteCollection routes)
		{
            routes.MapRoute("Plugin.Misc.Vendor.Test",
               "test/test",
               new { controller = "Test", action = "Test" },
               new[] { "Nop.Plugin.Misc.Vendor.Controllers" }
          );


            routes.MapLocalizedRoute("Register2",
				 "Catalog/CategoryNavigation",
				 new { controller = "Catalog", action = "Test" },
				 new[] { "Nop.Plugin.Misc.Vendor.Controllers" }
			);
		}

		public int Priority
		{
			get { return 100; }
		}
	}
}
