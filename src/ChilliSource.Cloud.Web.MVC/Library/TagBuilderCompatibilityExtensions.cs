#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    public static class TagBuilderCompatibilityExtensions
    {
#if NET_4X
        public static void SetInnerHtml(this TagBuilder tagBuilder, string innerHtml)
        {
            tagBuilder.InnerHtml = innerHtml;
        }

        public static void SetInnerHtml(this TagBuilder tagBuilder, IHtmlContent innerHtml)
        {
            tagBuilder.InnerHtml = innerHtml.ToHtmlString();
        }
#else
        public static void SetInnerText(this TagBuilder tagBuilder, string innerText)
        {
            tagBuilder.InnerHtml.Clear();
            tagBuilder.InnerHtml.Append(innerText);
        }
        
        public static void SetInnerHtml(this TagBuilder tagBuilder, string innerHtml)
        {
            tagBuilder.InnerHtml.Clear();
            tagBuilder.InnerHtml.AppendHtml(innerHtml);
        }

        public static void SetInnerHtml(this TagBuilder tagBuilder, IHtmlContent innerHtml)
        {
            tagBuilder.InnerHtml.Clear();
            tagBuilder.InnerHtml.AppendHtml(innerHtml);
        }
#endif
    }
}
