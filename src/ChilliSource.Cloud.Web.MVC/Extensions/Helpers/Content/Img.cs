using ChilliSource.Cloud.Core;
using System;
using System.IO;
using ChilliSource.Core.Extensions;
using System.Web;

#if NET_4X
using ChilliSource.Cloud.Core.Images;
using System.Web.Mvc;
#else
using SixLabors.ImageSharp;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Embeds image into page using src:data with base64 encoded image data.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="data">Raw image data.</param>
        /// <param name="altText">Optional alt text.</param>
        /// <param name="htmlAttributes">Optional attribute to include in the img tag.</param>
        /// <returns>An image tag with image encoded as base64.</returns>
#if NET_4X
        public static IHtmlContent ImgEmbedded(this HtmlHelper html, byte[] data, string altText = null, object htmlAttributes = null)
#else
        public static IHtmlContent ImgEmbedded(this IHtmlHelper html, byte[] data, string altText = null, object htmlAttributes = null)
#endif        
        {
            TagBuilder builder = new TagBuilder("img");

#if NET_4X
            var mimeType = data.ToImage().GetMimeType();
#else
            var format = Image.DetectFormat(data);
            var mimeType = format?.DefaultMimeType ?? "image/png";
#endif

            var base64Data = Convert.ToBase64String(data);
            builder.Attributes.Add("src", $"data:{mimeType};base64,{base64Data}");
            if (!String.IsNullOrEmpty(altText)) builder.Attributes.Add("alt", altText);

            if (htmlAttributes != null) builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return MvcHtmlStringCompatibility.Create(builder, TagRenderMode.SelfClosing);
        }
    }
}