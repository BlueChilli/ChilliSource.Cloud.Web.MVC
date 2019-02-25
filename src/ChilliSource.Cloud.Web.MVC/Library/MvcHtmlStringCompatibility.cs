#if NET_4X
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    internal static partial class MvcHtmlStringCompatibility
    {
        public static MvcHtmlString Empty()
        {
            return MvcHtmlString.Empty;
        }

        public static MvcHtmlString Create(IHtmlString content)
        {
            return content is MvcHtmlString ? (content as MvcHtmlString) : new MvcHtmlString(content.ToHtmlString());
        }

        public static MvcHtmlString Create(TagBuilder tagBuilder, TagRenderMode tagRenderMode)
        {
            return new MvcHtmlString(tagBuilder.ToString(tagRenderMode));
        }

        //public static MvcHtmlString Create(params IHtmlString[] contents)
        //{
        //    if (contents == null || contents.Length == 0)
        //        return MvcHtmlString.Empty;

        //    if (contents.Length == 1)
        //    {
        //        var first = contents[0];
        //        return (first is MvcHtmlString) ? (first as MvcHtmlString)
        //                : new MvcHtmlString(first.ToHtmlString());
        //    }

        //    StringBuilder sb = new StringBuilder();
        //    foreach (var content in contents)
        //    {
        //        sb.Append(content.ToHtmlString());
        //    }
        //    return new MvcHtmlString(sb.ToString());
        //}

        //Note: possibly return IHtmlString, so we can implement the composite pattern here as well
        public static MvcHtmlString Append(this MvcHtmlString thisMvcString, IHtmlString content)
        {
            if (content == MvcHtmlString.Empty)
            {
                return thisMvcString;
            }

            if (thisMvcString == MvcHtmlString.Empty)
            {
                return content is MvcHtmlString ? (content as MvcHtmlString) : new MvcHtmlString(content.ToHtmlString());
            }

            var strContent = thisMvcString.ToHtmlString() + content.ToHtmlString();
            return new MvcHtmlString(strContent);
        }

        public static MvcHtmlString Append(this MvcHtmlString thisMvcString, string value)
        {
            return thisMvcString.Append(Create(value));
        }

        //TODO: refactor dependencies and remove this method
        public static MvcHtmlString Create(string value)
        {
            return new MvcHtmlString(value);
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
        public static MvcHtmlString Empty()
        {
            return MvcHtmlStringImpl.Empty;
        }

        public static MvcHtmlString Create(IHtmlContent content)
        {
            return content is MvcHtmlString ? (content as MvcHtmlString) : new MvcHtmlStringImpl(content);
        }

        //public static MvcHtmlString Create(params IHtmlContent[] contents)
        //{
        //    if (contents == null || contents.Length == 0)
        //        return MvcHtmlStringImpl.Empty;

        //    if (contents.Length == 1)
        //        return new MvcHtmlStringImpl(contents[0]);

        //    var composite = new CompositeMvcHtmlString();
        //    composite.AppendRange(contents);

        //    return composite;
        //}

        public static MvcHtmlString Create(TagBuilder tagBuilder, TagRenderMode tagRenderMode)
        {
            tagBuilder.TagRenderMode = tagRenderMode;
            return Create(tagBuilder);
        }

        //TODO: refactor dependencies and remove this method
        public static MvcHtmlString Create(string value)
        {
            if (String.IsNullOrEmpty(value))
                return MvcHtmlStringImpl.Empty;

            return new MvcHtmlStringImpl(new HtmlString(value));
        }

        public static MvcHtmlString Append(this MvcHtmlString thisMvcString, IHtmlContent content)
        {
            if (content == MvcHtmlStringImpl.Empty)
            {
                return thisMvcString;
            }

            if (thisMvcString == MvcHtmlStringImpl.Empty)
            {
                return Create(content);
            }

            var composite = (thisMvcString as CompositeMvcHtmlString);
            if (composite == null)
            {
                composite = new CompositeMvcHtmlString();
                composite.Append(thisMvcString);
            }

            composite.Append(content);
            return composite;
        }

        public static MvcHtmlString Append(this MvcHtmlString thisMvcString, string value)
        {
            return thisMvcString.Append(Create(value));
        }
    }
}
#endif
