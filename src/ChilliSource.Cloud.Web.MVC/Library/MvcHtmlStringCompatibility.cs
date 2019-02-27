#if NET_4X
using System;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    internal static partial class MvcHtmlStringCompatibility
    {
        public static IHtmlContent Empty()
        {
            return MvcHtmlStringSimple.Empty;
        }

        public static IHtmlContent Create(string value)
        {
            if (String.IsNullOrEmpty(value))
                return MvcHtmlStringSimple.Empty;

            return new MvcHtmlStringSimple(value);
        }

        public static IHtmlContent AsHtmlContent(this IHtmlString content)
        {                        
            return content is IHtmlContent ? (content as IHtmlContent) : Create(content.ToHtmlString());
        }

        public static IHtmlContent Create(TagBuilder tagBuilder, TagRenderMode tagRenderMode)
        {
            return Create(tagBuilder.ToString(tagRenderMode));
        }
               
        public static IHtmlContent Append(this IHtmlContent thisMvcString, IHtmlString content)
        {
            if (content == MvcHtmlStringSimple.Empty)
            {
                return thisMvcString;
            }

            if (thisMvcString == MvcHtmlStringSimple.Empty)
            {
                return content.AsHtmlContent();
            }

            var composite = (thisMvcString as CompositeMvcHtmlString);
            if (composite == null)
            {
                composite = new CompositeMvcHtmlString();
                composite.AddElement(thisMvcString);
            }

            composite.AddElement(content.AsHtmlContent());
            return composite;
        }

        public static IHtmlContent Append(this IHtmlContent thisMvcString, string value)
        {
            return thisMvcString.Append(Create(value));
        }

        public static IHtmlContent AppendLine(this IHtmlContent thisMvcString, string value)
        {
            return thisMvcString.Append(Create(value)).Append(MvcHtmlStringSimple.NewLine);
        }
    }
}
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace ChilliSource.Cloud.Web.MVC
{
    internal static partial class MvcHtmlStringCompatibility
    {
        public static IHtmlContent Empty()
        {
            return MvcHtmlStringImpl.Empty;
        }

        //This method is needed due to code targeting .net framework 4.x
        public static IHtmlContent AsHtmlContent(this IHtmlContent content)
        {
            return content;
        }

        public static IHtmlContent Create(string value)
        {
            if (String.IsNullOrEmpty(value))
                return MvcHtmlStringImpl.Empty;

            return new MvcHtmlStringImpl(new HtmlString(value));
        }

        public static IHtmlContent Create(TagBuilder tagBuilder, TagRenderMode tagRenderMode)
        {
            tagBuilder.TagRenderMode = tagRenderMode;
            return tagBuilder;
        }

        public static IHtmlContent Append(this IHtmlContent thisMvcString, IHtmlContent content)
        {
            if (content == MvcHtmlStringImpl.Empty)
            {
                return thisMvcString;
            }

            if (thisMvcString == MvcHtmlStringImpl.Empty)
            {
                return content;
            }

            var composite = (thisMvcString as CompositeMvcHtmlString);
            if (composite == null)
            {
                composite = new CompositeMvcHtmlString();
                composite.AddElement(thisMvcString);
            }

            composite.AddElement(content);
            return composite;
        }

        public static IHtmlContent Append(this IHtmlContent thisMvcString, string value)
        {
            return thisMvcString.Append(Create(value));
        }

        public static IHtmlContent AppendLine(this IHtmlContent thisMvcString, string value)
        {
            return thisMvcString.Append(Create(value)).Append(MvcHtmlStringImpl.NewLine);
        }
    }
}
#endif
