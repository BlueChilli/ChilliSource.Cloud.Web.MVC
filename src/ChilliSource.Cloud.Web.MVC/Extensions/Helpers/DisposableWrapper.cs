using ChilliSource.Core.Extensions; using ChilliSource.Cloud.Core;
using System;
using System.Linq.Expressions;
#if NET_4X
using System.Web.Mvc;
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
    /// Defines methods to run when object created or disposed.
    /// </summary>
    internal class DisposableWrapper : IDisposable
    {
        private Action end;

        /// <summary>
        /// Initialise the object and run "begin" function.
        /// </summary>
        /// <param name="begin">Function to run when object created.</param>
        /// <param name="end">Function to run when object disposed.</param>
        public DisposableWrapper(Action begin, Action end)
        {
            this.end = end;
            begin();
        }

        /// <summary>
        /// When the object is disposed (end of using block), runs "end" function
        /// </summary>
        public void Dispose()
        {
            end();
        }
    }

    public static partial class HtmlHelperExtensions
    {

        /// <summary>
        /// Returns DisposableWrapper object to write HTML link begin tag when created and to write HTML link end tag when disposed.
        /// This is really just an example of how DisposableWrapper can be used to create Html helper methods.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="link">An HTML-encoded link.</param>
        /// <returns>A DisposableWrapper object.</returns>
        public static IDisposable BeginLink(this HtmlHelper htmlHelper, MvcHtmlString link)
        {
            return new DisposableWrapper(
                () => htmlHelper.ViewContext.Writer.Write(link.ToHtmlString().TrimEnd("</a>")),
                () => htmlHelper.ViewContext.Writer.Write("</a>")
            );
        }
    }
}