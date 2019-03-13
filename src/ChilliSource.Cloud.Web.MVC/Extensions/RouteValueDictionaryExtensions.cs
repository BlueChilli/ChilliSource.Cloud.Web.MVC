using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;

#if NET_4X
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    public static class AttributeExtensions
    {

#if NET_4X
        public static IHtmlContent ToAttributeHtmlContent(this RouteValueDictionary dictionary)
        {
            var stringValue = String.Join(" ", dictionary.Keys.Select(
                                key => String.Format("{0}=\"{1}\"", key,
                                dictionary[key])));

            return MvcHtmlStringCompatibility.Create(stringValue);
        }
#else
        public static IHtmlContent ToAttributeHtmlContent(this RouteValueDictionary dictionary)
        {
            return new DictionaryAttributeHtmlContent<string, object>(dictionary);
        }

        internal class DictionaryAttributeHtmlContent<TKey, TValue> : IHtmlContent
        {
            IDictionary<TKey, TValue> _routes;
            public DictionaryAttributeHtmlContent(IDictionary<TKey, TValue> routes)
            {
                _routes = routes;
            }

            public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            {
                var keys = _routes.Keys;
                var length = keys.Count;
                var count = 0;
                foreach (var key in keys)
                {
                    var keyStr = key.ToString();
                    writer.Write(keyStr);
                    writer.Write("=\"");

                    var value = _routes[key];
                    if (value is IHtmlContent)
                    {
                        (value as IHtmlContent).WriteTo(writer, encoder);
                    }
                    else
                    {
                        var valueStr = value.ToString();
                        writer.Write(valueStr);
                    }

                    count++;

                    if (count < length)
                    {
                        writer.Write("\" ");
                    }
                    else
                    {
                        writer.Write("\"");
                    }
                }
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
#endif
    }    
}
