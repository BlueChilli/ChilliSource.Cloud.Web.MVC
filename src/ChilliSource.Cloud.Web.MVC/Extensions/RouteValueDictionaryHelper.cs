using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NET_4X
using System.Web.Mvc;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// See RouteValueDictionaryExtensions in ChilliSource.Cloud.Web for additional functions.
    /// </summary>
    public class RouteValueDictionaryHelper
    {
        /// <summary>
        /// Converts html attribute object to System.Web.Routing.RouteValueDictionary.
        /// </summary>
        /// <param name="htmlAttributes">The specified html attribute object.</param>
        /// <returns>A System.Web.Routing.RouteValueDictionary.</returns>
        public static RouteValueDictionary CreateFromHtmlAttributes(object htmlAttributes)
        {
            RouteValueDictionary result;

            if (htmlAttributes == null)
                return new RouteValueDictionary();

            if (htmlAttributes is RouteValueDictionary)
                result = htmlAttributes as RouteValueDictionary;
            else if (htmlAttributes is IDictionary<string, object>)
                result = new RouteValueDictionary(htmlAttributes as IDictionary<string, object>);
            else
            {
#if NET_4X
                result = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
#else
                result = new RouteValueDictionary(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
#endif
            }

            result = result ?? new RouteValueDictionary();

            if (result.ContainsKey("disabled") && result["disabled"] is Boolean && ((bool)result["disabled"]) == false)
            {
                result.Remove("disabled");
            }

            return result;
        }
    }
}