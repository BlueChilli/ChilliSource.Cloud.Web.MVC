#if NETCOREAPP

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliSource.Cloud.Web.MVC
{
    public static class NavigationHelpers
    {
        public static string GetCurrentArea(this IHtmlHelper html)
        {
            return html.ViewContext.RouteData.Values.GetValueOrDefault("area") as string;
        }
    }
}
#endif
