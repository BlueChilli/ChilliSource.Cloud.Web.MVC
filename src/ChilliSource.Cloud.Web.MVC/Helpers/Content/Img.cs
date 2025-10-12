using ChilliSource.Cloud.Core;
using System;
using System.IO;
using ChilliSource.Core.Extensions;
using System.Web;
using SixLabors.ImageSharp;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

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
        public static IHtmlContent ImgEmbedded(this IHtmlHelper html, byte[] data, string altText = null, object htmlAttributes = null)
        {
            TagBuilder builder = new TagBuilder("img");

            var format = Image.DetectFormat(data);
            var mimeType = format?.DefaultMimeType ?? "image/png";

            var base64Data = Convert.ToBase64String(data);
            builder.Attributes.Add("src", $"data:{mimeType};base64,{base64Data}");
            if (!String.IsNullOrEmpty(altText)) builder.Attributes.Add("alt", altText);

            if (htmlAttributes != null) builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return MvcHtmlStringCompatibility.Create(builder, TagRenderMode.SelfClosing);
        }
    }
}