#if NET_4X
using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    ///  This interface makes code targeting .net 4.x compatible with .net standard 2.0
    /// </summary>
    public interface IHtmlContent : IHtmlString
    {

    }

    internal static partial class MvcHtmlStringCompatibility
    {
        private class SimpleMvcHtmlString : IHtmlContent
        {
            public static readonly IHtmlContent Empty = new SimpleMvcHtmlString(String.Empty);
            public static readonly IHtmlContent NewLine = new SimpleMvcHtmlString("\r\n");

            private string _htmlString;

            public SimpleMvcHtmlString(string htmlString)
            {
                _htmlString = htmlString ?? String.Empty;
            }

            public string ToHtmlString()
            {
                return _htmlString;
            }

            public override string ToString()
            {
                throw new ApplicationException("SimpleMvcHtmlString.ToString() is not intended to be used");
            }
        }

        private class CompositeMvcHtmlString : IHtmlContent
        {
            private List<IHtmlContent> _contentList = new List<IHtmlContent>();

            public CompositeMvcHtmlString() { }

            public void AddElement(IHtmlContent content)
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

            public void AddElements(IEnumerable<IHtmlContent> contents)
            {
                foreach (var content in contents)
                {
                    //using append method to flatten structure if needed
                    this.AddElement(content);
                }
            }

            public string ToHtmlString()
            {
                var sb = new StringBuilder();
                foreach (var content in _contentList)
                {
                    sb.Append(content.ToHtmlString());
                }

                return sb.ToString();
            }

            public override string ToString()
            {
                throw new ApplicationException("CompositeMvcHtmlString.ToString() is not intended to be used");
            }

            private string DebuggerToString()
            {
                return this.ToHtmlString();
            }
        }
    }
}
#else
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;

namespace ChilliSource.Cloud.Web.MVC
{
    internal static partial class MvcHtmlStringCompatibility
    {
        private class SimpleMvcHtmlString : HtmlString
        {
            public SimpleMvcHtmlString(string value)
                : base(value ?? String.Empty) { }

            public override string ToString()
            {
                throw new ApplicationException("SimpleMvcHtmlString.ToString() is not intended to be used");
            }

            private string DebuggerToString()
            {
                return base.Value;
            }
        }

        private class CompositeMvcHtmlString : IHtmlContent
        {
            private List<IHtmlContent> _contentList = new List<IHtmlContent>();

            public CompositeMvcHtmlString() { }

            public void AddElement(IHtmlContent content)
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

            public void AddElements(IEnumerable<IHtmlContent> contents)
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

            private string DebuggerToString()
            {
                using (var writer = new StringWriter())
                {
                    this.WriteTo(writer, HtmlEncoder.Default);
                    return writer.ToString();
                }
            }
        }
    }
}
#endif
