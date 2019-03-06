using System;
using System.Text;

#if NET_4X
using System.Web.Mvc;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns string for HTML attributes.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <returns>An HTML-encoded string for HTML attributes.</returns>
#if NET_4X
        public static IHtmlContent Attributes<TModel>(this HtmlHelper<TModel> html, object htmlAttributes)
#else
        public static IHtmlContent Attributes<TModel>(this IHtmlHelper<TModel> html, object htmlAttributes)
#endif        
        {
            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(htmlAttributes);
            var result = MvcHtmlStringCompatibility.Empty();
            foreach (var key in attributes.Keys)
            {
                result = result.Append(String.Format(@" {0}=""{1}""", key, attributes[key]));
            }

            return result;
        }
    }
}