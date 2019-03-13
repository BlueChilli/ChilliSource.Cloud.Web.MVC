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
        public static IUrlHelper GetUrlHelper(this IHtmlHelper htmlHelper)
        {
            IServiceProvider serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
            var urlHelperFactory = serviceProvider.GetRequiredService<IUrlHelperFactory>();

            // should we use GetRequiredService<IActionContextAccessor>().ActionContext ?
            var urlHelper = urlHelperFactory.GetUrlHelper(htmlHelper.ViewContext);
            return urlHelper;
        }
#endif
    }
}