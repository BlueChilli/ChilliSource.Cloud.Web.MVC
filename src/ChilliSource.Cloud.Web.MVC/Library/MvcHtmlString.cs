#if !NET_4X
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    ///  This interface makes code targeting .net 4.x compatible with .net standard 2.0
    /// </summary>
    public interface MvcHtmlString : IHtmlContent
    {

    }

    internal class MvcHtmlStringImpl : MvcHtmlString
    {
        public static readonly MvcHtmlString Empty = MvcHtmlStringCompatibility.Create(String.Empty);

        IHtmlContent _inner;
        internal MvcHtmlStringImpl(IHtmlContent inner)
        {
            _inner = inner;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _inner.WriteTo(writer, encoder);
        }

        public override string ToString()
        {
            throw new ApplicationException("MvcHtmlString.ToString() is not intended to be used");
        }
    }

    internal class CompositeMvcHtmlString : MvcHtmlString
    {
        private List<IHtmlContent> _contentList = new List<IHtmlContent>();

        public CompositeMvcHtmlString() { }

        public void Append(IHtmlContent content)
        {
            if (content is CompositeMvcHtmlString)
            {
                //flattens structure when adding a composite.
                _contentList.AddRange((content as CompositeMvcHtmlString)._contentList);
            }
            else
            {
                _contentList.Add(content);
            }
        }

        public void AppendRange(IEnumerable<IHtmlContent> contents)
        {
            foreach (var content in contents)
            {
                //using append method to flatten structure if needed
                this.Append(content);
            }
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            foreach (var content in _contentList)
            {
                content.WriteTo(writer, encoder);
            }
        }

        public override string ToString()
        {
            throw new ApplicationException("CompositeMvcHtmlString.ToString() is not intended to be used");
        }
    }
}
#endif
