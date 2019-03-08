using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NET_4X
using System.Web.Mvc;
using System.Web.Mvc.Html;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class TemplateHelper
    {
        #region Container Template
#if NET_4X

        private static FieldTemplateContent CreateTemplateContent(HtmlHelper html, string template, object model, string folder = "Templates")
        {
            var content = html.Partial($"{folder}/{template}", model).AsHtmlContent();

            return new FieldTemplateContent(content);
        }

        public static IHtmlContent Template(this HtmlHelper html, TemplateType template, object model = null, string folder = "Templates")
        {
            return html.Template(template.ToString(), model, folder);
        }

        public static IHtmlContent Template(this HtmlHelper html, string template, object model = null, string folder = "Templates")
        {
            return html.Partial($"{folder}/{template}", model).AsHtmlContent();
        }

        public static IDisposable ContainerTemplate(this HtmlHelper html, TemplateType template, object model = null, string folder = "Templates")
        {
            return html.ContainerTemplate(template.ToString(), model, folder);
        }

        public static IDisposable ContainerTemplate(this HtmlHelper html, string template, object model = null, string folder = "Templates")
        {
            var templateContent = CreateTemplateContent(html, template, model, folder);

            return new DisposableWrapper(
                () => html.ViewContext.Writer.Write(templateContent.BeginContent().ToHtmlString()),
                () => html.ViewContext.Writer.Write(templateContent.EndContent().ToHtmlString()));
        }

        public static IHtmlContent ContainerTemplateBegin(HtmlHelper html, string template, object model, string folder = "Templates")
        {
            return CreateTemplateContent(html, template, model, folder).BeginContent();
        }

        public static IHtmlContent ContainerTemplateEnd(HtmlHelper html, string template, object model, string folder = "Templates")
        {
            return CreateTemplateContent(html, template, model, folder).EndContent();
        }

#else
        private static async Task<FieldTemplateContent> CreateTemplateContentAsync(IHtmlHelper html, string template, object model, string folder = "Templates")
        {
            var content = await html.PartialAsync($"{folder}/{template}", model);

            return new FieldTemplateContent(content);
        }

        public static Task<IHtmlContent> TemplateAsync(this IHtmlHelper html, TemplateType template, object model = null, string folder = "Templates")
        {
            return html.TemplateAsync(template.ToString(), model, folder);
        }

        public static Task<IHtmlContent> TemplateAsync(this IHtmlHelper html, string template, object model = null, string folder = "Templates")
        {
            return html.PartialAsync($"{folder}/{template}", model);
        }

        public static Task<IDisposable> ContainerTemplateAsync(this IHtmlHelper html, TemplateType template, object model = null, string folder = "Templates")
        {
            return html.ContainerTemplateAsync(template.ToString(), model, folder);
        }

        public static async Task<IDisposable> ContainerTemplateAsync(this IHtmlHelper html, string template, object model = null, string folder = "Templates")
        {
            var templateContent = await CreateTemplateContentAsync(html, template, model, folder);

            return new DisposableWrapper(html, () => templateContent.BeginContent(), () => templateContent.EndContent());
        }

        public static async Task<IHtmlContent> ContainerTemplateBeginAsync(IHtmlHelper html, string template, object model, string folder = "Templates")
        {
            return (await CreateTemplateContentAsync(html, template, model, folder)).BeginContent();
        }

        public static async Task<IHtmlContent> ContainerTemplateEnd(IHtmlHelper html, string template, object model, string folder = "Templates")
        {
            return (await CreateTemplateContentAsync(html, template, model, folder)).EndContent();
        }
#endif
        #endregion
    }

    internal class FieldTemplateContent
    {        
        IHtmlContent _beginHtmlContent;
        IHtmlContent _endHtmlContent;

        public int ContentLength { get; private set; }

        internal FieldTemplateContent(IHtmlContent content)
        {
#if NET_4X
            var htmlContentStr = content.ToHtmlString();
#else
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                content.WriteTo(writer, HtmlEncoder.Default);
            }

            var htmlContentStr = sb.ToString();
#endif

            this.ContentLength = htmlContentStr.Length;

            var index = htmlContentStr.IndexOf(FieldTemplateModel.InnerTemplateMarker);
            if (index >= 0)
            {
                _beginHtmlContent = MvcHtmlStringCompatibility.Create(htmlContentStr.Substring(0, index));
                _endHtmlContent = MvcHtmlStringCompatibility.Create(htmlContentStr.Substring(index + FieldTemplateModel.InnerTemplateMarker.Length));
            }
            else
            {
                _beginHtmlContent = MvcHtmlStringCompatibility.Empty();
                _endHtmlContent = MvcHtmlStringCompatibility.Empty();
            }
        }

        public IHtmlContent BeginContent()
        {
            return _beginHtmlContent;
        }

        public IHtmlContent EndContent()
        {
            return _endHtmlContent;
        }
    }

#if NET_4X
    internal class FieldTemplateHtmlContent : IHtmlContent
    {
        FieldTemplateContent _templateContent;
        IHtmlContent _innerContent;

        public FieldTemplateHtmlContent(FieldTemplateContent templateContent, IHtmlContent innerContent)
        {
            _templateContent = templateContent;
            _innerContent = innerContent;
        }

        public string ToHtmlString()
        {
            var sb = new StringBuilder(capacity: _templateContent.ContentLength);
            using (var writer = new StringWriter(sb))
            {
                writer.Write(_templateContent.BeginContent().ToHtmlString());
                writer.Write(_innerContent.ToHtmlString());
                writer.Write(_templateContent.EndContent().ToHtmlString());
                writer.Flush();
            }

            return sb.ToString();
        }
    }
#else
    internal class FieldTemplateHtmlContent : IHtmlContent
    {
        FieldTemplateContent _templateContent;
        IHtmlContent _innerContent;

        public FieldTemplateHtmlContent(FieldTemplateContent templateContent, IHtmlContent innerContent)
        {
            _templateContent = templateContent;
            _innerContent = innerContent;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _templateContent.BeginContent().WriteTo(writer, encoder);
            _innerContent.WriteTo(writer, encoder);
            _templateContent.EndContent().WriteTo(writer, encoder);
        }
    }
#endif
}