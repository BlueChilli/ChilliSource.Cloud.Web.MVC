using System;
using System.Text;
#if NET_4X
using System.Web.Mvc;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
#if NET_4X
        public static UrlHelper GetUrlHelper(this HtmlHelper htmlHelper)
        {
            return new UrlHelper(htmlHelper.ViewContext.RequestContext);
        }
#else
        public static UrlHelper GetUrlHelper(this IHtmlHelper htmlHelper)
        {
            IServiceProvider serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
            var urlHelperFactory = serviceProvider.GetRequiredService<IUrlHelperFactory>();

            // TODO: should we use GetRequiredService<IActionContextAccessor>().ActionContext ?
            var urlHelper = urlHelperFactory.GetUrlHelper(htmlHelper.ViewContext);
            return new UrlHelper(urlHelper);
        }
#endif
    }

#if !NET_4X
    public class UrlHelper : IUrlHelper
    {
        IUrlHelper _inner;
        internal UrlHelper(IUrlHelper inner)
        {
            _inner = inner;
        }

        public ActionContext ActionContext => _inner.ActionContext;

        public string Action(UrlActionContext actionContext)
        {
            return _inner.Action(actionContext);
        }

        public string Content(string contentPath)
        {
            return _inner.Content(contentPath);
        }

        public bool IsLocalUrl(string url)
        {
            return _inner.IsLocalUrl(url);
        }

        public string Link(string routeName, object values)
        {
            return _inner.Link(routeName, values);
        }

        public string RouteUrl(UrlRouteContext routeContext)
        {
            return _inner.RouteUrl(routeContext);
        }
    }
#endif
}