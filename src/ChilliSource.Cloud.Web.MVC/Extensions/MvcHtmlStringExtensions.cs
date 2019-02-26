using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Extension methods for System.Web.Mvc.IHtmlContent.
    /// </summary>
    public static class IHtmlContentExtensions
    {
#if NET_4X
        /// <summary>
        /// Creates a new HTML-encoded string by formatting the specified HTML-encoded string.
        /// </summary>
        /// <param name="value">The specified HTML-encoded string.</param>
        /// <param name="format">A composite format string.</param>
        /// <returns>An HTML-encoded string.</returns>
        public static IHtmlContent Format(this IHtmlContent value, string format)
        {
            //This cannot be ported to .net core without compromising performance
            return MvcHtmlStringCompatibility.Create(String.Format(format, value.ToHtmlString()));
        }        
#endif
        [Obsolete("Bad method design. value parameter is not used")]
        public static IHtmlContent Format(this IHtmlContent value, string format, params object[] args)
        {
            return MvcHtmlStringCompatibility.Create(String.Format(format, args));
        }

        /// <summary>
        /// Creates a new HTML-encoded string by formatting the specified HTML-encoded string.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="values">An object array that contains zero or more objects to format.</param>
        /// <returns>An HTML-encoded string.</returns>
        public static IHtmlContent Format(string format, params object[] values)
        {
            return MvcHtmlStringCompatibility.Create(String.Format(format, values));
        }
    }
}