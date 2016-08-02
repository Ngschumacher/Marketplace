using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.Vendor
{
    public class VendorPlugin : BasePlugin, IWidgetPlugin
    {
        #region Methods

        public IList<string> GetWidgetZones()
        {
            return new List<string>() {"left_side_column_before"};
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "Test";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Misc.Vendor.Controllers" }, { "area", null }, };
        }
        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Test";
            controllerName = "Test";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.Misc.Vendor.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }
        #endregion
    }
}
