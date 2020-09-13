using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

#if NET_4X
using System.Web.Mvc;
using System.Web.Mvc.Html;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.DataProtection;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for validation summary message.
        /// </summary>
        /// <param name="html">An object that contains the HTML attributes.</param>
        /// <param name="validationMessage">Validation message.</param>
        /// <returns>An HTML string for validation summary message.</returns>\
#if NET_4X
        public static IHtmlContent ValidationSummaryHtml(this HtmlHelper html, string validationMessage)
#else
        public static IHtmlContent ValidationSummaryHtml(this IHtmlHelper html, string validationMessage)
#endif
        {
            return html.ValidationSummary(validationMessage).AsHtmlContent();
        }
    }
}