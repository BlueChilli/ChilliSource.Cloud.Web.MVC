using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
#if NET_4X
using System.Web.Mvc;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Represents support for creating modal window.
    /// </summary>
    public static class ModalHelper
    {
        /// <summary>
        /// Creates model container which will host a partial view.
        /// </summary>
        /// <param name="helper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="id">Id of modal.</param>
        /// <param name="title">The title for modal window.</param>
        /// <param name="showClose">True to display close button, otherwise not.</param>
        /// <param name="showPrint">True to display print button, otherwise not.</param>
        /// <returns>An HTML string for the modal window.</returns>
        public static MvcHtmlString ModalContainer(this HtmlHelper helper, string id, string title = "", bool showClose = true, bool showPrint = false)
        {
            return MvcHtmlStringCompatibility.Create(ModalContainer(id, title, showClose, showPrint));
        }

        /// <summary>
        /// Creates model container which will host a partial view.
        /// </summary>
        /// <param name="id">Id of modal.</param>
        /// <param name="title">The title for modal window.</param>
        /// <param name="showClose">True to display close button, otherwise not.</param>
        /// <param name="showPrint">True to display print button, otherwise not.</param>
        /// <returns>The string for the modal window.</returns>
        public static string ModalContainer(string id, string title = "", bool showClose = true, bool showPrint = false)
        {
            string format = @"<div id=""{0}"" class=""modal hide""><div class=""modal-header"">{1}{2}{3}</div><div class=""modal-body"" id=""{0}_content""></div></div>";
            string titleFormat = "<h3>{0}</h3>";
            string close = @"<a class=""close"" data-dismiss=""modal"">×</a>";
            string print = @"<a class=""print"" onclick=""window.print();""></a>";

            if (!showClose) close = "";
            if (!showPrint) print = "";
            titleFormat = String.IsNullOrEmpty(title) ? "" : String.Format(titleFormat, title);

            return String.Format(format, id, close, print, titleFormat);
        }

 
        private static string AppendJsonData(string source, string value)
        {
            if (!string.IsNullOrEmpty(source))
                return String.Format("{0}, {1}", source, value);

            return value;
        }      
    }
}