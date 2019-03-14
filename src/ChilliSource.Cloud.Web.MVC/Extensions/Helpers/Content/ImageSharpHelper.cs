#if !NET_4X
using ChilliSource.Cloud.Core;
using System;
using System.IO;
using ChilliSource.Core.Extensions;
using System.Web;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    public class ImageSharpHelper
    {
        private IRemoteStorage _remoteStorage;
        private string _prefix;
        IUrlHelper _urlHelper;

        public ImageSharpHelper(IRemoteStorage remoteStorage, IUrlHelper urlHelper, string prefix)
        {
            _remoteStorage = remoteStorage;
            _urlHelper = urlHelper;

            if (String.IsNullOrEmpty(prefix))
            {
                throw new ArgumentNullException("prefix is required");
            }

            if (!prefix.StartsWith("~"))
            {
                throw new ApplicationException("Prefix must be relative and start with ~");
            }

            prefix = prefix.TrimEnd("/");
            this._prefix = prefix;
        }

        /// <summary>
        /// Returns HTML string for the cloud image element (Image stored in the Cloud - s3 or azure).
        /// </summary>        
        /// <param name="filename">The name of image file.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="altText">The alternate text of the image.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the image element.</param>
        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
        /// <returns>An HTML-encoded string for the image element.</returns>
        public IHtmlContent Image(string filename, int? width = null, int? height = null, string altText = null, object htmlAttributes = null, string alternativeImage = "")
        {
            return Image(filename, new ImageSharpCommand { Width = width, Height = height }, altText, htmlAttributes, alternativeImage);
        }

        /// <summary>
        /// Returns HTML string for the cloud image element (Image stored in the Cloud - s3 or azure).
        /// </summary>
        /// <param name="filename">The name of image file.</param>
        /// <param name="cmd">The ImageSharpCommand for the width and height of the image.</param>
        /// <param name="altText">The alternate text of the image.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the image element.</param>
        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
        /// <param name="ensureSize">Specifies whether width and height attributes should be generated in the 'img' tag. Defaults to true (compatibility).</param>
        /// <returns>An HTML-encoded string for image element.</returns>
        public IHtmlContent Image(string filename, ImageSharpCommand cmd, string altText = null, object htmlAttributes = null, string alternativeImage = "", bool ensureSize = true)
        {
            return ImageLocal(ImageUrl(filename), cmd, altText, htmlAttributes, alternativeImage, ensureSize);
        }

        /// <summary>
        /// Returns HTML string for the image element.
        /// </summary>
        /// <param name="filename">The name of image file.</param>
        /// <param name="cmd">The ImageSharpCommand for the width and height of the image.</param>
        /// <param name="altText">The alternate text of the image.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the image element.</param>
        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
        /// <returns>An HTML-encoded string for the image element.</returns>
        public IHtmlContent ImageLocal(string filename, ImageSharpCommand cmd, string altText = null, object htmlAttributes = null, string alternativeImage = "", bool ensureSize = true)
        {
            var url = ImageSharpQuery(ResolveFilenameToUrl(filename, alternativeImage, isLocal: true), cmd);

            if (cmd == null) cmd = new ImageSharpCommand();

            TagBuilder builder = new TagBuilder("img");
            builder.Attributes.Add("src", url);
            if (ensureSize)
            {
                if (cmd.Width.HasValue) builder.Attributes.Add("width", cmd.Width.Value.ToString());
                if (cmd.Height.HasValue) builder.Attributes.Add("height", cmd.Height.Value.ToString());
            }

            if (!String.IsNullOrEmpty(altText)) builder.Attributes.Add("alt", altText);
            if (htmlAttributes != null) builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

            return MvcHtmlStringCompatibility.Create(builder, TagRenderMode.SelfClosing);
        }

        /// <summary>
        /// Returns the Root-Relative URL for the specified image file.
        /// </summary>
        /// <param name="filename">The file name of the image.</param>
        /// <returns>A Root-Relative URL for the specified image file.</returns>
        public string ImageUrl(string filename)
        {
            return ResolveFilenameToUrl(filename);
        }

        /// <summary>
        /// Returns a Root-Relative URL with image resize query parameters for the image file stored in remote storage.
        /// </summary>
        /// <param name="filename">The name of the image file.</param>
        /// <param name="cmd">The ImageSharpCommand.</param>
        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
        /// <param name="fullPath">If true returns the absolute uri eg https://www.mysite.com</param>
        /// <returns>A Root-Relative URL with image resize query parameters for the image file in the remote storage.</returns>
        public string ImageUrl(string filename, ImageSharpCommand cmd = null, string alternativeImage = null, bool fullPath = false)
        {
            return ImageSharpQuery(ResolveFilenameToUrl(filename, alternativeImage, fullPath: fullPath), cmd);
        }

        private string ResolveFilenameToUrl(string filename, string alternativeImage = "", bool isLocal = false, bool fullPath = false)
        {
            filename = StringExtensions.DefaultTo(filename, alternativeImage);
            if (String.IsNullOrEmpty(filename)) return "";

            if (filename.StartsWith("~"))
            {
                return fullPath ? _urlHelper.ParseUri(filename).AbsoluteUri : _urlHelper.ParseUri(filename).AbsolutePath;
            }
            else if (filename.StartsWith("http://") || filename.StartsWith("https://") || filename.StartsWith("//"))
            {
                return filename;
            }
            else
            {
                var uri = _urlHelper.ParseUri($"{this._prefix}/{this._remoteStorage.GetPartialFilePath(filename)}");
                return isLocal ? filename : fullPath ? uri.AbsoluteUri : uri.AbsolutePath;
            }
        }

        /// <summary>
        /// Returns CSS background property with S3 image stored in Amazon S3 storage.
        /// </summary>
        /// <param name="filename">The name of image file.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="norepeat">True to set "no-repeat" value in CSS property, otherwise not.</param>
        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
        /// <returns>An HTML-encoded string for CSS background property.</returns>
        public IHtmlContent BackgroundImage(string filename, int? width, int? height, bool norepeat = true, string alternativeImage = null)
        {
            return BackgroundImage(filename, new ImageSharpCommand { Width = width, Height = height }, norepeat, alternativeImage);
        }

        /// <summary>
        /// Returns CSS background property with S3 image stored in Amazon S3 storage.
        /// </summary>
        /// <param name="filename">The name of image file.</param>
        /// <param name="cmd">The ImageSharpCommand for the width and height of the image.</param>
        /// <param name="norepeat">True to set "no-repeat" value in CSS property, otherwise not.</param>
        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
        /// <returns>An HTML-encoded string for CSS background property.</returns>
        public IHtmlContent BackgroundImage(string filename, ImageSharpCommand cmd, bool norepeat = true, string alternativeImage = null)
        {
            var url = ImageUrl(filename, cmd, alternativeImage);
            return MvcHtmlStringCompatibility.Empty().AppendFormat("background: url('{0}'){1}; height: {2}px; width: {3}px;", url, norepeat ? " no-repeat" : "", cmd.Height, cmd.Width);
        }

        /// <summary>
        /// Appends image resize query parameters to the image file name.
        /// </summary>
        /// <param name="filename">The name of the image file.</param>
        /// <param name="cmd">The ImageSharpCommand.</param>
        /// <returns>An image file name with image resize query parameters appended.</returns>
        public string ImageSharpQuery(string filename, ImageSharpCommand cmd)
        {
            if (cmd == null || (cmd.Height == null && cmd.Width == null))
                return filename;

            var query = new QueryBuilder();

            if (cmd.Width.HasValue) query.Add("width", cmd.RetinaWidth().Value.ToString());
            if (cmd.Height.HasValue) query.Add("height", cmd.RetinaHeight().Value.ToString());

            if (cmd.CenterXY != null && cmd.CenterXY.Length == 2) query.Add("rxy", String.Join(",", cmd.CenterXY));
            if (cmd.Anchor != AnchorPositionMode.Center) query.Add("ranchor", cmd.Anchor.ToString().ToLower());
            if (cmd.Mode != ResizeMode.Crop) query.Add("rmode", cmd.Mode.ToString().ToLower());
            if (cmd.Compand) query.Add("compand", cmd.Compand.ToString().ToLower());
            if (!String.IsNullOrWhiteSpace(cmd.ResizingSampler)) query.Add("rsampler", cmd.ResizingSampler.ToLower());

            if (cmd.Format != ImageSharpFormat.Original) query.Add("format", cmd.Format.ToString().ToLower());
            if (!String.IsNullOrWhiteSpace(cmd.BgColor)) query.Add("bgcolor", cmd.BgColor);

            if (cmd.Quality.HasValue) query.Add("quality", cmd.Quality.Value.ToString());

            if (cmd.Rotate.HasValue && cmd.Rotate.Value != 0) query.Add("rotate", cmd.Rotate.Value.ToString());
            if (cmd.AutoOrient) query.Add("autoorient", "true");

            //TODO: blur parameter (and implement WebProcessor)
            //if (cmd.Blur != 0) query.Add("blur", cmd.Blur.ToString());

            return String.Format("{0}{1}", filename, query.ToQueryString());
        }
    }

    #region Image Resizer
    /// <summary>
    /// Represents the commands used by image resize.
    /// </summary>
    /// <remarks>https://github.com/SixLabors/ImageSharp.Web</remarks>
    public class ImageSharpCommand
    {
        /// <summary>
        /// Initialize a new instance of ImageSharpCommand. See individual properties for default values.
        /// </summary>
        public ImageSharpCommand() { }

        /// <summary>
        /// Gets or set the image width.
        /// </summary>
        public int? Width { get; set; }
        /// <summary>
        /// Gets or sets the image height.
        /// </summary>
        public int? Height { get; set; }

        public float[] CenterXY { get; set; }

        /// <summary>
        /// Gets or sets the background color to use when resizing (e.g 000000 or black)
        /// </summary>
        public string BgColor { get; set; }

        /// <summary>
        /// Gets or sets the image resize mode. It defaults to ResizeMode.Pad.
        /// </summary>
        public ResizeMode Mode { get; set; } = ResizeMode.Pad;

        public AnchorPositionMode Anchor { get; set; }

        public bool Compand { get; set; }

        public string ResizingSampler { get; set; }

        public ImageSharpFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the scale options for retina screen. 
        /// </summary>
        public ImageRetinaScale RetinaScale { get; set; }

        public int? Quality { get; set; }

        public bool AutoOrient { get; set; } = true;

        public int? Rotate { get; set; }

        /// <summary>
        /// Gets the image width for retina screen.
        /// </summary>
        /// <returns>The width of the image.</returns>
        internal int? RetinaWidth()
        {
            if (Width.HasValue)
            {
                switch (RetinaScale)
                {
                    case ImageRetinaScale.Double: return Width.Value * 2;
                    default: return Width.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the image height for the retina screen.
        /// </summary>
        /// <returns>The height of the image.</returns>
        internal int? RetinaHeight()
        {
            if (Height.HasValue)
            {
                switch (RetinaScale)
                {
                    case ImageRetinaScale.Double: return Height.Value * 2;
                    default: return Height.Value;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Enumeration values for ImageSharpFormat. The output format to use.
    /// </summary>
    public enum ImageSharpFormat
    {
        /// <summary>
        /// Do not specify options for ImageSharpFormat, keep the original format.
        /// </summary>
        Original,
        /// <summary>
        /// Outputs image in JPG format.
        /// </summary>
        JPG,
        /// <summary>
        /// Outputs image in GIF format.
        /// </summary>
        GIF,
        /// <summary>
        /// Outputs image in PNG format.
        /// </summary>
        PNG,
    }

    /// <summary>
    /// Enumeration values for ImageRetinaScale. Returns larger image and use CSS to constrain image to actual size. Retina screens will make use of the extra pixels.
    /// </summary>
    public enum ImageRetinaScale
    {
        /// <summary>
        /// Do not specify options for ImageRetinaScale
        /// </summary>
        None,
        /// <summary>
        /// Doubles the image height and width.
        /// </summary>
        Double
    }
    #endregion
}
#endif