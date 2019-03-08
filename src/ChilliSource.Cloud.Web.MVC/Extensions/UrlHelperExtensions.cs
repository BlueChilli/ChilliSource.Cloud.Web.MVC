using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

#if NET_4X
using System.Web.Mvc;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Extension methods for System.Web.Mvc.UrlHelper.
    /// </summary>
    public static class UrlHelperExtensions
    {
#if NET_4X
        /// <summary>
        /// Creates a new instance from the current HTTP context.
        /// </summary>
        /// <returns></returns>
        public static UrlHelper Create()
        {
            var httpContext = HttpContext.Current;

            if (httpContext == null)
            {
                var request = new HttpRequest("/", GlobalWebConfiguration.Instance.BaseUrl, "");
                var response = new HttpResponse(new StringWriter());
                httpContext = new HttpContext(request, response);
            }

            var httpContextBase = new HttpContextWrapper(httpContext);
            var routeData = new RouteData();
            var requestContext = new RequestContext(httpContextBase, routeData);

            UrlHelper url = new UrlHelper(requestContext);

            return url;
        }
#endif

        #region Current
        /// <summary>
        /// Gets action from the HTTP request in the specified System.Web.Mvc.UrlHelper.
        /// </summary>
        /// <param name="urlHelper">The specified System.Web.Mvc.UrlHelper.</param>
        /// <returns>A string value of action.</returns>        
#if NET_4X
        public static string CurrentAction(this UrlHelper urlHelper)
        {
            return RouteHelper.CurrentAction(urlHelper.RequestContext);
        }
#else
        public static string CurrentAction(this IUrlHelper urlHelper)
        {
            return RouteHelper.CurrentAction(urlHelper.ActionContext.RouteData.Values);
        }
#endif

        /// <summary>
        /// Gets controller from the HTTP request in the specified System.Web.Mvc.UrlHelper.
        /// </summary>
        /// <param name="urlHelper">The specified System.Web.Mvc.UrlHelper.</param>
        /// <returns>A string value of controller.</returns>        
#if NET_4X
        public static string CurrentController(this UrlHelper urlHelper)
        {
            return RouteHelper.CurrentController(urlHelper.RequestContext);
        }
#else
        public static string CurrentController(this IUrlHelper urlHelper)
        {
            return RouteHelper.CurrentController(urlHelper.ActionContext.RouteData.Values);
        }
#endif


        /// <summary>
        /// Gets area from the HTTP request in the specified System.Web.Mvc.UrlHelper.
        /// </summary>
        /// <param name="urlHelper">The specified System.Web.Mvc.UrlHelper.</param>
        /// <returns>A string value of area.</returns>
#if NET_4X
        public static string CurrentArea(this UrlHelper urlHelper)
        {
            return RouteHelper.CurrentArea(urlHelper.RequestContext);
        }
#else
        public static string CurrentArea(this IUrlHelper urlHelper)
        {
            return RouteHelper.CurrentArea(urlHelper.ActionContext.RouteData.Values);
        }
#endif


#if NET_4X
        /// <summary>
        /// Checks if the specified URL or the route data match the URL or route data in the System.Web.Mvc.UrlHelper.
        /// </summary>
        /// <param name="urlHelper">The specified System.Web.Mvc.UrlHelper.</param>
        /// <param name="url">The specified URL string.</param>
        /// <returns>True when the specified URL or the route data match the URL or route data in the System.Web.Mvc.UrlHelper, otherwise false.</returns>
        public static bool IsCurrent(this UrlHelper urlHelper, string url)
        {
            if (urlHelper.RequestContext.HttpContext.Request.RawUrl.Equals(url, StringComparison.OrdinalIgnoreCase)) return true;

            var requestRoute = urlHelper.RequestContext.RouteData.Values;
            var compareRoute = RouteHelper.GetRouteDataByUrl(url);
            foreach (string key in compareRoute.Values.Keys)
            {
                if (requestRoute.Keys.Contains(key) && RouteHelper.Keys.Contains(key))
                {
                    if (!requestRoute[key].ToString().Equals(compareRoute.Values[key].ToString(), StringComparison.OrdinalIgnoreCase))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns HTML-encoded string of the specified result string when the specified URL is the current URL in System.Web.Mvc.UrlHelper.
        /// </summary>
        /// <param name="urlHelper">The specified System.Web.Mvc.UrlHelper.</param>
        /// <param name="url">The specified URL.</param>
        /// <param name="resultWhenTrue">An HTML-encoded string.</param>
        /// <returns>An HTML-encoded string of the specified result string when the specified URL is the current URL in System.Web.Mvc.UrlHelper, otherwise HTML-encoded string of empty string.</returns>
        public static IHtmlContent WhenIsCurrent(this UrlHelper urlHelper, string url, string resultWhenTrue)
        {
            return IsCurrent(urlHelper, url) ? MvcHtmlStringCompatibility.Create(resultWhenTrue) : MvcHtmlStringCompatibility.Empty();
        }
#endif

        /// <summary>
        /// Checks if the specified area and controller names are same as area and controller names in System.Web.Mvc.UrlHelper.
        /// </summary>
        /// <param name="urlHelper">The specified System.Web.Mvc.UrlHelper.</param>
        /// <param name="controller">The specified controller name.</param>
        /// <param name="area">The specified area name.</param>
        /// <returns>Trhe when the specified area and controller names are same as area and controller names in System.Web.Mvc.UrlHelper, otherwise false.</returns>
        public static bool IsCurrent(this UrlHelper urlHelper, string controller, string area)
        {
            return IsCurrentArea(urlHelper, area) && IsCurrentController(urlHelper, controller);
        }

        /// <summary>
        /// Checks if the specified area name is same as area name in System.Web.Mvc.UrlHelper.
        /// </summary>
        /// <param name="urlHelper">The specified System.Web.Mvc.UrlHelper.</param>
        /// <param name="areaName">>The specified area name.</param>
        /// <returns>True when the specified area name is same as area name in System.Web.Mvc.UrlHelper, otherwise false.</returns>
        public static bool IsCurrentArea(this UrlHelper urlHelper, string areaName)
        {
            if (areaName == null) areaName = "";
            return areaName.Equals(CurrentArea(urlHelper), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if the specified controller name is same as controller name in System.Web.Mvc.UrlHelper.
        /// </summary>
        /// <param name="urlHelper">The specified System.Web.Mvc.UrlHelper.</param>
        /// <param name="controller">The specified controller name.</param>
        /// <returns>True when the specified controller name is same as controller name in System.Web.Mvc.UrlHelper, otherwise false.</returns>
        public static bool IsCurrentController(this UrlHelper urlHelper, string controller)
        {
            if (controller == null) controller = "";
            return controller.Equals(CurrentController(urlHelper), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns HTML-encoded string of the specified result string when the specified controller name is the current controller name in System.Web.Mvc.UrlHelper.
        /// </summary>
        /// <param name="urlHelper">The specified System.Web.Mvc.UrlHelper.</param>
        /// <param name="controller">The specified controller name.</param>
        /// <param name="resultWhenTrue">An HTML-encoded string.</param>
        /// <returns>An HTML-encoded string of the specified result string when the specified controller name is the current controller name in System.Web.Mvc.UrlHelper, otherwise HTML-encoded string of empty string.</returns>
        public static IHtmlContent WhenIsCurrentController(this UrlHelper urlHelper, string controller, string resultWhenTrue)
        {
            return IsCurrentController(urlHelper, controller) ? MvcHtmlStringCompatibility.Create(resultWhenTrue) : MvcHtmlStringCompatibility.Create("");
        }
        #endregion

        /// <summary>
        /// Generates a fully qualified URL for an action method by using the specified action name, controller name, route values, protocol, and host name or generates the URL for the specified route values by using the specified route name, protocol, and host name.
        /// </summary>
        /// <param name="urlHelper">The specified System.Web.Mvc.UrlHelper.</param>
        /// <param name="actionName">The name of the action method.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <param name="areaName">The name of the area.</param>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="id">The value of ID in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route.</param>
        /// <param name="protocol">The protocol for the URL, such as "http" or "https".</param>
        /// <param name="hostName">The host name for the URL.</param>
        /// <param name="fragment">A fragment identifier (%23).</param>
        /// <returns>The fully qualified URL to an action method.</returns>
#if NET_4X
        public static string DefaultAction(this UrlHelper urlHelper, string actionName, string controllerName = "", string areaName = null, string routeName = "", string id = null, object routeValues = null, string protocol = "", string hostName = "", string fragment = "")
#else
        public static string DefaultAction(this IUrlHelper urlHelper, string actionName, string controllerName = "", string areaName = null, string routeName = "", string id = null, object routeValues = null, string protocol = "", string hostName = "", string fragment = "")
#endif        
        {
            controllerName = controllerName.DefaultTo(urlHelper.CurrentController());
            areaName = (areaName == null) ? urlHelper.CurrentArea() : areaName; //String.Empty will remove area

            var routeValuesDictionary = routeValues as RouteValueDictionary;
            if (routeValuesDictionary == null) routeValuesDictionary = new RouteValueDictionary(routeValues);
            routeValuesDictionary = FixEnumerableRouteDataValues(routeValuesDictionary);

            if (String.IsNullOrEmpty(routeName))
            {
                routeValuesDictionary["area"] = StringExtensions.DefaultTo((string)routeValuesDictionary["area"], areaName);
            }
            else
            {
                var routeAction = StringExtensions.DefaultTo((string)routeValuesDictionary["action"], actionName);
                if (!String.IsNullOrEmpty(routeAction)) routeValuesDictionary["action"] = routeAction;

                var routeController = StringExtensions.DefaultTo((string)routeValuesDictionary["controller"], controllerName);
                if (!String.IsNullOrEmpty(routeController)) routeValuesDictionary["controller"] = routeController;
            }

            //default protocol to current protocol if hostname set, but protocol isn't
            if (!String.IsNullOrEmpty(hostName) && String.IsNullOrEmpty(protocol))
            {
#if NET_4X
                protocol = urlHelper.RequestContext.HttpContext.Request.Url.Scheme;
#else
                protocol = urlHelper.ActionContext.HttpContext.Request.Scheme;
#endif
            }

            if (!String.IsNullOrEmpty(id)) routeValuesDictionary["id"] = id;

            string href = String.IsNullOrEmpty(routeName) ? urlHelper.Action(actionName, controllerName, routeValuesDictionary, protocol, hostName) : urlHelper.RouteUrl(routeName, routeValuesDictionary, protocol, hostName);
            href = href + (String.IsNullOrEmpty(fragment) ? "" : $"#{fragment}");

            return href;
        }

        private static RouteValueDictionary FixEnumerableRouteDataValues(RouteValueDictionary routes)
        {
            var result = new RouteValueDictionary();
            foreach (var key in routes.Keys)
            {
                object value = routes[key];
                if (value is System.Collections.IEnumerable && !(value is string))
                {
                    int index = 0;
                    foreach (var val in (System.Collections.IEnumerable)value)
                    {
                        result.Add(string.Format("{0}[{1}]", key, index), val);
                        index++;
                    }
                }
                else
                {
                    result.Add(key, value);
                }
            }

            return result;
        }


        /// <summary>
        /// Generates a System.Uri for an action method by using the specified action name, controller name, route values and protocol or generates the System.Uri for the specified route values by using the specified route name and protocol.
        /// </summary>
        /// <param name="urlHelper">The specified System.Web.Mvc.UrlHelper.</param>
        /// <param name="actionName">The name of the action method.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <param name="areaName">The name of the area.</param>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="id">The value of ID in the route values.</param>
        /// <param name="routeValues">An object that contains the parameters for a route.</param>
        /// <param name="protocol">The protocol for the URL, such as "http" or "https".</param>
        /// <returns>A System.Uri.</returns>
        public static Uri DefaultUri(this UrlHelper urlHelper, string actionName, string controllerName = "", string areaName = null, string routeName = "", string id = null, object routeValues = null, string protocol = "")
        {
            return UriExtensions.Parse(DefaultAction(urlHelper, actionName, controllerName, areaName, routeName, id, routeValues, protocol));
        }

#if NET_4X
        /// <summary>
        /// Generates a fully qualified URL by using URI schema and authority segments from the specified System.Web.Mvc.UrlHelper, URL string and protocol. 
        /// </summary>
        /// <param name="urlHelper">The specified System.Web.Mvc.UrlHelper.</param>
        /// <param name="url">The specified URL string.</param>
        /// <param name="protocol">The protocol for the URL, such as "http" or "https".</param>
        /// <returns>A fully qualified URL.</returns>
        public static string GenerateExternalUrl(this UrlHelper urlHelper, string url, string protocol = "")
        {
            if (url.StartsWith("~"))
            {
                var httpContext = urlHelper.RequestContext.HttpContext;
                var appPath = httpContext.Request.ApplicationPath ?? new Uri(GlobalWebConfiguration.Instance.BaseUrl).PathAndQuery;
                url = VirtualPathUtility.ToAbsolute(url, appPath);
            }
            url = urlHelper.RequestContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + url;
            if (!String.IsNullOrEmpty(protocol)) url = url.Replace(urlHelper.RequestContext.HttpContext.Request.Url.Scheme, protocol);
            return url;
        }
#endif
    }
}