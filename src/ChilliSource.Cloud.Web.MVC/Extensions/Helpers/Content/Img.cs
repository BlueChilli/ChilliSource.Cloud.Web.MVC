using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Images;
using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Web;
using System.Linq;
using System.Text;
using ChilliSource.Core.Extensions;

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
        public static MvcHtmlString ImgEmbedded(this HtmlHelper html, byte[] data, string altText = null, object htmlAttributes = null)
        {
            TagBuilder builder = new TagBuilder("img");

            var mimeType = data.ToImage().GetMimeType();
            var base64Data = Convert.ToBase64String(data);
            builder.Attributes.Add("src", $"data:{mimeType};base64,{base64Data}");
            if (!String.IsNullOrEmpty(altText)) builder.Attributes.Add("alt", altText);

            if (htmlAttributes != null) builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }
    }

    public class HtmlHelperImageResizer
    {
        private IRemoteStorage _remoteStorage;
        private string _prefix;
        private HtmlHelper html;
        private Lazy<UrlHelper> urlHelper;

        public HtmlHelperImageResizer(HtmlHelper html, IRemoteStorage remoteStorage, string prefix)
        {
            this._remoteStorage = remoteStorage;

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

            this.html = html;
            this.urlHelper = new Lazy<UrlHelper>(() =>
            {
                return new UrlHelper(this.html.ViewContext.RequestContext);
            });
        }

        /// <summary>
        /// Returns HTML string for the image element.
        /// </summary>        
        /// <param name="filename">The name of image file.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="altText">The alternate text of the image.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the image element.</param>
        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
        /// <returns>An HTML-encoded string for the image element.</returns>
        public MvcHtmlString Image(string filename, int? width = null, int? height = null, string altText = null, object htmlAttributes = null, string alternativeImage = "")
        {
            return Image(filename, new ImageResizerCommand { Width = width, Height = height, AutoRotate = false }, altText, htmlAttributes, alternativeImage);
        }

        /// <summary>
        /// Returns HTML string for the image element.
        /// </summary>
        /// <param name="filename">The name of image file.</param>
        /// <param name="cmd">The ImageResizerCommand for the width and height of the image.</param>
        /// <param name="altText">The alternate text of the image.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the image element.</param>
        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
        /// <returns>An HTML-encoded string for the image element.</returns>
        public MvcHtmlString ImageLocal(string filename, ImageResizerCommand cmd, string altText = null, object htmlAttributes = null, string alternativeImage = "")
        {
            TagBuilder builder = new TagBuilder("img");

            builder.Attributes.Add("src", ImageResizerQuery(ResolveFilenameToUrl(DirectoryType.Images, filename, alternativeImage), cmd));
            if (!String.IsNullOrEmpty(altText)) builder.Attributes.Add("alt", altText);
            if (cmd.Width.HasValue) builder.Attributes.Add("width", cmd.Width.Value.ToString());
            if (cmd.Height.HasValue) builder.Attributes.Add("height", cmd.Height.Value.ToString());
            if (htmlAttributes != null) builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }

        /// <summary>
        /// Returns the fully qualified URL for the specified image file.
        /// </summary>
        /// <param name="filename">The file name of the image.</param>
        /// <returns>A fully qualified URL for the specified image file.</returns>
        public string ImageUrl(string filename)
        {
            return ResolveFilenameToUrl(DirectoryType.Images, filename);
        }

        private string ResolveFilenameToUrl(DirectoryType directoryType, string filename, string alternativeImage = "")
        {
            string url = "";
            filename = StringExtensions.DefaultTo(filename, alternativeImage);
            if (String.IsNullOrEmpty(filename)) return "";

            if (filename.StartsWith("~"))
            {
                url = urlHelper.Value.Content(filename);
            }
            else if (filename.StartsWith("http://") || filename.StartsWith("https://") || filename.StartsWith("//"))
            {
                url = filename;
            }
            else
            {
                url = GlobalMVCConfiguration.Instance.GetPath(directoryType, filename);
            }

            return urlHelper.Value.Content(url);
        }

        private string ResolveProtocol(string url, string protocol)
        {
            return String.IsNullOrEmpty(protocol) ? url : urlHelper.Value.GenerateExternalUrl(url, protocol);
        }

        /// <summary>
        /// Returns HTML string for the cloud image element (Image stored in the Cloud - s3 or azure).
        /// </summary>
        /// <param name="filename">The name of image file.</param>
        /// <param name="cmd">The ImageResizerCommand for the width and height of the image.</param>
        /// <param name="altText">The alternate text of the image.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the image element.</param>
        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
        /// <param name="ensureSize">Specifies whether width and height attributes should be generated in the 'img' tag. Defaults to true (compatibility).</param>
        /// <returns>An HTML-encoded string for image element.</returns>
        public MvcHtmlString Image(string filename, ImageResizerCommand cmd, string altText = null, object htmlAttributes = null, string alternativeImage = "", bool ensureSize = true)
        {
            if (cmd == null) cmd = new ImageResizerCommand();
            if (String.IsNullOrEmpty(filename))
                return Image(ImageResizerQuery(alternativeImage, cmd), cmd.Width, cmd.Height, altText, htmlAttributes);

            TagBuilder builder = new TagBuilder("img");

            var path = StoragePath(filename, cmd, alternativeImage: alternativeImage);
            var url = urlHelper.Value.Content(path);

            builder.Attributes.Add("src", url);
            if (ensureSize)
            {
                if (cmd.Width.HasValue) builder.Attributes.Add("width", cmd.Width.Value.ToString());
                if (cmd.Height.HasValue) builder.Attributes.Add("height", cmd.Height.Value.ToString());
            }

            if (!String.IsNullOrEmpty(altText)) builder.Attributes.Add("alt", altText);
            if (htmlAttributes != null) builder.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
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
        public MvcHtmlString BackgroundImage(string filename, int? width, int? height, bool norepeat = true, string alternativeImage = null)
        {
            return BackgroundImage(filename, new ImageResizerCommand { Width = width, Height = height }, norepeat, alternativeImage);
        }

        /// <summary>
        /// Returns CSS background property with S3 image stored in Amazon S3 storage.
        /// </summary>
        /// <param name="filename">The name of image file.</param>
        /// <param name="cmd">The ImageResizerCommand for the width and height of the image.</param>
        /// <param name="norepeat">True to set "no-repeat" value in CSS property, otherwise not.</param>
        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
        /// <returns>An HTML-encoded string for CSS background property.</returns>
        public MvcHtmlString BackgroundImage(string filename, ImageResizerCommand cmd, bool norepeat = true, string alternativeImage = null)
        {
            var path = String.IsNullOrEmpty(filename) ? ImageUrl(ImageResizerQuery(alternativeImage, cmd)) : StoragePath(filename, cmd, alternativeImage: alternativeImage);
            var url = urlHelper.Value.Content(path);
            return MvcHtmlString.Empty.Format("background: url('{0}'){1}; height: {2}px; width: {3}px;", url, norepeat ? " no-repeat" : "", cmd.Height, cmd.Width);
        }        

        /// <summary>
        ///     Returns the url the Azure image (Image stored in Azure storage).
        /// </summary>
        /// <param name="filename">The name of image file.</param>
        /// <param name="cmd">The ImageResizerCommand for the width and height of the image.</param>
        /// <param name="protocol">The protocol of the URL ("http" or "https").</param>
        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
        /// <returns>The Azure image url</returns>
        public IHtmlString ImageUrl(string filename, ImageResizerCommand cmd, string protocol = "", string alternativeImage = "")
        {
            var path = StoragePath(filename, cmd, protocol, alternativeImage);
            return html.Raw(ImageUrl(path));
        }


        /// <summary>
        /// Returns a fully qualified URL with image resize query parameters for the image file stored in Azure.
        /// </summary>
        /// <param name="filename">The name of the image file.</param>
        /// <param name="cmd">The ImageResizerCommand.</param>
        /// <param name="protocol">The protocol of the URL ("http" or "https").</param>
        /// <param name="alternativeImage">The alternate image if filename is empty or null.</param>
        /// <returns>A fully qualified URL with image resize query parameters for the image file in the remote storage.</returns>
        public string StoragePath(string filename, ImageResizerCommand cmd = null, string protocol = "", string alternativeImage = null)
        {
            if (String.IsNullOrEmpty(filename) && String.IsNullOrEmpty(alternativeImage))
            {
                return "";
            }

            var url = String.IsNullOrEmpty(filename) ? ResolveProtocol(alternativeImage, protocol)
                        : ImagePathWithoutQuery(filename, protocol);

            return ImageResizerQuery(url, cmd);
        }

        private string ImagePathWithoutQuery(string fileName, string protocol = "")
        {
            var path = _remoteStorage.GetPartialFilePath(fileName);
            var url = ResolveProtocol(path, protocol);

            return url;
        }

        /// <summary>
        /// Appends image resize query parameters to the image file name.
        /// </summary>
        /// <param name="filename">The name of the image file.</param>
        /// <param name="cmd">The ImageResizerCommand.</param>
        /// <returns>An image file name with image resize query parameters appended.</returns>
        public string ImageResizerQuery(string filename, ImageResizerCommand cmd)
        {
            if (cmd == null)
                return filename;

            var query = HttpUtility.ParseQueryString(string.Empty);
            if (cmd.Height.HasValue) query.Add("h", cmd.RetinaHeight().Value.ToString());
            if (cmd.Width.HasValue) query.Add("w", cmd.RetinaWidth().Value.ToString());
            if (cmd.Mode != ImageResizerMode.Pad) query.Add("mode", cmd.Mode.ToString().ToLower());
            if (cmd.Anchor != ImageResizerAnchor.None) query.Add("anchor", cmd.Anchor.ToString().ToLower());
            if (cmd.Scale != ImageResizerScale.None) query.Add("scale", cmd.Scale.ToString().ToLower());
            if (cmd.Format != ImageResizerFormat.Original) query.Add("format", cmd.Format.ToString().ToLower());
            if (cmd.Quality.HasValue && cmd.Quality.Value != 90 && (cmd.Format == ImageResizerFormat.JPG || Path.GetExtension(filename).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(filename).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)))
                query.Add("quality", cmd.Quality.Value.ToString());
            if (cmd.Rotate.HasValue && cmd.Rotate.Value != 0) query.Add("rotate", cmd.Rotate.Value.ToString());
            if (cmd.AutoRotate) query.Add("autorotate", "true");
            if (cmd.Blur != 0) query.Add("blur", cmd.Blur.ToString());
            if (!String.IsNullOrEmpty(cmd.BgColor)) query.Add("bgcolor", cmd.BgColor);

            return String.Format("{0}?{1}", filename, query.ToString());
        }   
    }

    #region Image Resizer
    /// <summary>
    /// Represents the commands used by image resize.
    /// </summary>
    /// <remarks>http://imageresizing.net/docs/reference</remarks>
    public class ImageResizerCommand
    {
        /// <summary>
        /// Initialize a new instance of ImageResizerCommand with "AutoRotate" property set to True.
        /// </summary>
        public ImageResizerCommand()
        {
            AutoRotate = true;
        }

        /// <summary>
        /// Gets or set the image width.
        /// </summary>
        public int? Width { get; set; }
        /// <summary>
        /// Gets or sets the image height.
        /// </summary>
        public int? Height { get; set; }
        /// <summary>
        /// Gets or sets the image resize mode.
        /// </summary>
        public ImageResizerMode Mode { get; set; }
        /// <summary>
        /// Gets or sets how to anchor the image for padding or cropping mode.
        /// </summary>
        public ImageResizerAnchor Anchor { get; set; }
        /// <summary>
        /// Gets or sets the scale options when image resizing.
        /// </summary>
        public ImageResizerScale Scale { get; set; }
        /// <summary>
        /// Gets or sets the format of the image.
        /// </summary>
        public ImageResizerFormat Format { get; set; }
        /// <summary>
        /// Gets or sets the scale options for retina screen. 
        /// </summary>
        public ImageRetinaScale RetinaScale { get; set; }
        /// <summary>
        /// Gets or sets the image quality, only if format is JPG or has been forced to JPG, default is 90.
        /// </summary>
        public int? Quality { get; set; }
        /// <summary>
        /// Gets or sets degrees to rotate the image.
        /// </summary>
        public double? Rotate { get; set; }
        /// <summary>
        /// Gets or sets the "AutoRotate" property which automatically rotates the image based on the EXIF info from the camera (Requires the AutoRotate plugin).
        /// </summary>
        /// <remarks>http://imageresizing.net/plugins/autorotate</remarks>
        public bool AutoRotate { get; set; }

        /// <summary>
        /// Gets or sets the radius for Gaussian blur.
        /// </summary>
        public int Blur { get; set; }

        /// <summary>
        /// Gets or sets the background color to use when resizing (e.g 000000 or black)
        /// </summary>
        public string BgColor { get; set; }

        /// <summary>
        /// Gets the image width for retina screen.
        /// </summary>
        /// <returns>The width of the image.</returns>
        public int? RetinaWidth()
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
        public int? RetinaHeight()
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
    /// The enumeration values for ImageResizerMode. How to handle aspect-ratio conflicts between the image and width+height.
    /// </summary>
    public enum ImageResizerMode
    {
        /// <summary>
        /// Adds whitespace.
        /// </summary>
        Pad,
        /// <summary>
        /// Behaves like max width/max height.
        /// </summary>
        Max,
        /// <summary>
        /// Crops minimally.
        /// </summary>
        Crop,
        /// <summary>
        /// Loses aspect-ratio, stretching the image
        /// </summary>
        Stretch,
        /// <summary>
        /// Uses seam carving, requires SeamCarving plugin.
        /// </summary>
        /// <remarks>http://imageresizing.net/plugins/seamcarving</remarks>
        Carve
    }

    /// <summary>
    /// Enumeration values for ImageResizerAnchor. How to anchor the image when padding or cropping.
    /// </summary>
    public enum ImageResizerAnchor
    {
        /// <summary>
        /// Do not specify options for ImageResizerAnchor.
        /// </summary>
        None,
        /// <summary>
        /// Image at top left.
        /// </summary>
        TopLeft,
        /// <summary>
        /// Image at top center.
        /// </summary>
        TopCenter,
        /// <summary>
        /// Image at top right.
        /// </summary>
        TopRight,
        /// <summary>
        /// Image at middle left.
        /// </summary>
        MiddleLeft,
        /// <summary>
        /// Image at middle center.
        /// </summary>
        MiddleCenter,
        /// <summary>
        /// Image at middle right.
        /// </summary>
        MiddleRight,
        /// <summary>
        /// Image at bottom left.
        /// </summary>
        BottomLeft,
        /// <summary>
        /// Image at bottom center.
        /// </summary>
        BottomCenter,
        /// <summary>
        /// Image ate bottom right.
        /// </summary>
        BottomRight
    }

    /// <summary>
    /// Enumeration values for ImageResizerScale. By default, images are not enlarged - the image stays its original size if you request a larger size.
    /// </summary>
    public enum ImageResizerScale
    {
        /// <summary>
        /// Do not specify options for ImageResizerScale.
        /// </summary>
        None,
        /// <summary>
        /// Reduce the size of the image.
        /// </summary>
        Down,
        /// <summary>
        /// Allow both reduction and enlargement.
        /// </summary>
        Both,
        /// <summary>
        /// Expands image to fill the desired area.
        /// </summary>
        Canvas
    }

    /// <summary>
    /// Enumeration values for ImageResizerFormat. The output format to use.
    /// </summary>
    public enum ImageResizerFormat
    {
        /// <summary>
        /// Do not specify options for ImageResizerFormat, keep the original format.
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