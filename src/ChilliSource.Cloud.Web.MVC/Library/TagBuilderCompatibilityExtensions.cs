#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    internal static class TagBuilderCompatibilityExtensions
    {
#if NET_4X
        public static void SetInnerHtml(this TagBuilder tagBuilder, string innerHtml)
        {
            tagBuilder.InnerHtml = innerHtml;
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
#endif
    }
}
