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

        private static FieldTemplateContent CreateTemplateContent(HtmlHelper html, TemplateType template, object model)
        {
            var content = html.Partial($"{template.ViewPath}", model).AsHtmlContent();

            return new FieldTemplateContent(content);
        }

        public static IHtmlContent Template(this HtmlHelper html, TemplateType template, object model = null)
        {
            return html.Partial($"{template.ViewPath}", model).AsHtmlContent();
        }

        public static IDisposable ContainerTemplate(this HtmlHelper html, TemplateType template, object model = null)
        {
            var templateContent = CreateTemplateContent(html, template, model);

            return new DisposableWrapper(
                () => html.ViewContext.Writer.Write(templateContent.BeginContent().ToHtmlString()),
                () => html.ViewContext.Writer.Write(templateContent.EndContent().ToHtmlString()));
        }

        public static IHtmlContent ContainerTemplateBegin(HtmlHelper html, TemplateType template, object model)
        {
            return CreateTemplateContent(html, template, model).BeginContent();
        }

        public static IHtmlContent ContainerTemplateEnd(HtmlHelper html, TemplateType template, object model)
        {
            return CreateTemplateContent(html, template, model).EndContent();
        }

#else
        private static async Task<FieldTemplateContent> CreateTemplateContentAsync(this IHtmlHelper html, TemplateType template, object model)
        {
            var content = await html.PartialAsync($"{template.ViewPath}", model);

            return new FieldTemplateContent(content);
        }        

        public static Task<IHtmlContent> TemplateAsync(this IHtmlHelper html, TemplateType template, object model = null)
        {
            return html.PartialAsync($"{template.ViewPath}", model);
        }

        public static async Task<IDisposable> ContainerTemplateAsync(this IHtmlHelper html, TemplateType template, object model = null)
        {
            var templateContent = await CreateTemplateContentAsync(html, template, model);

            return new DisposableWrapper(html, () => templateContent.BeginContent(), () => templateContent.EndContent());
        }

        public static async Task<IHtmlContent> ContainerTemplateBeginAsync(IHtmlHelper html, TemplateType template, object model)
        {
            return (await CreateTemplateContentAsync(html, template, model)).BeginContent();
        }

        public static async Task<IHtmlContent> ContainerTemplateEnd(IHtmlHelper html, TemplateType template, object model)
        {
            return (await CreateTemplateContentAsync(html, template, model)).EndContent();
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