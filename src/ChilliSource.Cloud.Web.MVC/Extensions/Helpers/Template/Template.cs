using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class TemplateHelper
    {
        public const string InnerTemplateMarker = "###InnerTemplate###";

        /// <summary>
        /// Render a template base on passed template type and model.
        /// To render a custom project template create another helper for MyTemplateType for example.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="template">Template to render</param>
        /// <param name="model">Depending on template rendered model may be mandatory.</param>
        /// <returns></returns>
        public static MvcHtmlString Template(this HtmlHelper html, TemplateType template, object model = null)
        {
            return ContainerTemplateBegin(html, template.ToString(), model);
        }

        #region Container Template
        public static IDisposable ContainerTemplate(this HtmlHelper html, TemplateType template, object model = null)
        {
            return ContainerTemplate(html, template.ToString(), model);
        }

        public static IDisposable ContainerTemplate(this HtmlHelper html, string template, object model = null)
        {
            return new DisposableWrapper(
                () => html.ViewContext.Writer.Write(ContainerTemplateBegin(html, template, model)),
                () => html.ViewContext.Writer.Write(ContainerTemplateEnd(html, template, model))
            );
        }

        public static MvcHtmlString ContainerTemplateBegin(HtmlHelper html, string template, object model, string folder = "Templates")
        {
            var content = html.Partial($"{folder}/{template}", model).ToString();

            if (content.Contains(InnerTemplateMarker))
            {
                content = content.Substring(0, content.IndexOf(InnerTemplateMarker));
            }

            return MvcHtmlString.Create(content);
        }

        public static MvcHtmlString ContainerTemplateEnd(HtmlHelper html, string template, object model, string folder = "Templates")
        {
            var content = html.Partial($"{folder}/{template}", model).ToString();

            if (content.Contains(InnerTemplateMarker))
            {
                content = content.Substring(content.IndexOf(InnerTemplateMarker) + InnerTemplateMarker.Length);
            }
            else
            {
                return MvcHtmlString.Empty;
            }
            return MvcHtmlString.Create(content);
        }
        #endregion
    }
}
    
