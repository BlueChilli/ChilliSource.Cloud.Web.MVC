﻿#if NET_4X
using System;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class MvcHtmlStringCompatibility
    {
        private static readonly IHtmlContent _empty = new SimpleMvcHtmlString(String.Empty);
        private static readonly IHtmlContent _newLine = new SimpleMvcHtmlString("\r\n");

        public static IHtmlContent Empty()
        {
            return _empty;
        }

        public static IHtmlContent Create(string value)
        {
            if (String.IsNullOrEmpty(value))
                return _empty;

            return new SimpleMvcHtmlString(value);
        }

        public static IHtmlContent AsHtmlContent(this IHtmlString content)
        {
            return content is IHtmlContent ? (content as IHtmlContent) : Create(content.ToHtmlString());
        }

        public static IHtmlContent AsHtmlContent(this TagBuilder content, TagRenderMode tagRenderMode = TagRenderMode.Normal)
        {
            return Create(content, tagRenderMode);
        }

        public static IHtmlContent Create(TagBuilder tagBuilder, TagRenderMode tagRenderMode)
        {
            return Create(tagBuilder.ToString(tagRenderMode));
        }
               
        public static IHtmlContent Append(this IHtmlContent thisMvcString, IHtmlString content)
        {
            if (content == _empty)
            {
                return thisMvcString;
            }

            if (thisMvcString == _empty)
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
            return thisMvcString.Append(Create(value)).Append(_newLine);
        }

        public static IHtmlContent AppendLine(this IHtmlContent thisMvcString)
        {
            return thisMvcString.Append(_newLine);
        }
    }
}
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class MvcHtmlStringCompatibility
    {
        private static readonly IHtmlContent _newLine = new SimpleMvcHtmlString("\r\n");

        public static IHtmlContent Empty()
        {
            return HtmlString.Empty;
        }

        //This method is needed due to code targeting .net framework 4.x
        public static IHtmlContent AsHtmlContent(this IHtmlContent content)
        {
            return content;
        }

        public static IHtmlContent AsHtmlContent(this TagBuilder content, TagRenderMode tagRenderMode = TagRenderMode.Normal)
        {
            return Create(content, tagRenderMode);
        }

        public static IHtmlContent Create(string value)
        {
            if (String.IsNullOrEmpty(value))
                return HtmlString.Empty;

            return new SimpleMvcHtmlString(value);
        }

        public static IHtmlContent Create(TagBuilder tagBuilder, TagRenderMode tagRenderMode)
        {
            tagBuilder.TagRenderMode = tagRenderMode;
            return tagBuilder;
        }

        public static IHtmlContent Append(this IHtmlContent thisMvcString, IHtmlContent content)
        {
            if (content == HtmlString.Empty)
            {
                return thisMvcString;
            }

            if (thisMvcString == HtmlString.Empty)
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
            return thisMvcString.Append(Create(value)).Append(_newLine);
        }

        public static IHtmlContent AppendLine(this IHtmlContent thisMvcString)
        {
            return thisMvcString.Append(_newLine);
        }
    }
}
#endif