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
        public static IHtmlContent AppendFormat(this IHtmlContent value, string format, params object[] args)
        {
            return value.Append(String.Format(format, args));
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