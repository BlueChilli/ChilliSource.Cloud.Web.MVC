using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using System;
using System.Linq.Expressions;

#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;
#endif

namespace ChilliSource.Cloud.Web.MVC
{

#if NET_4X
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
        public static IDisposable BeginLink(this HtmlHelper htmlHelper, IHtmlContent link)
        {
            return new DisposableWrapper(
                () => htmlHelper.ViewContext.Writer.Write(link.ToHtmlString().TrimEnd("</a>")),
                () => htmlHelper.ViewContext.Writer.Write("</a>")
            );
        }
    }
#else

    /// <summary>
    /// Defines methods to run when object created or disposed.
    /// </summary>
    internal class DisposableWrapper : IDisposable
    {
        ViewContext _viewContext;
        HtmlEncoder _htmlEncoder;
        Func<IHtmlContent> _endContent;

        public DisposableWrapper(IHtmlHelper htmlHelper, Func<IHtmlContent> beginContent, Func<IHtmlContent> endContent)
            : this(htmlHelper.ViewContext, HtmlEncoder.Default, beginContent, endContent)
        {
        }

        public DisposableWrapper(ViewContext viewContext, HtmlEncoder htmlEncoder, Func<IHtmlContent> beginContent, Func<IHtmlContent> endContent)
        {
            _viewContext = viewContext;
            _htmlEncoder = htmlEncoder;
            _endContent = endContent;

            beginContent()?.WriteTo(_viewContext.Writer, _htmlEncoder);
        }

        /// <summary>
        /// When the object is disposed (end of using block), runs "endContent" function
        /// </summary>
        public void Dispose()
        {
            var writer = _viewContext?.Writer;
            if (writer == null)
                return;

            var encoder = _htmlEncoder;
            if (encoder == null)
                return;

            var endContent = _endContent;
            if (endContent == null)
                return;

            endContent()?.WriteTo(writer, encoder);

            _viewContext = null;
            _htmlEncoder = null;
            _endContent = null;

        }
    }    
#endif
}