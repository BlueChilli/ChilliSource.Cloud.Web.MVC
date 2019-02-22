#if !NET_4X
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;

namespace ChilliSource.Cloud.Web.MVC
{
    public class MvcHtmlString : IHtmlContent
    {
        public static readonly MvcHtmlString Empty = new MvcHtmlString(new HtmlString(String.Empty));

        IHtmlContent _inner;
        internal MvcHtmlString(IHtmlContent inner)
        {
            _inner = inner;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _inner.WriteTo(writer, encoder);
        }

        public static MvcHtmlString Create(string value)
        {
            if (String.IsNullOrEmpty(value))
                return Empty;

            return new MvcHtmlString(new HtmlString(value));
        }
    }
}
#endif
