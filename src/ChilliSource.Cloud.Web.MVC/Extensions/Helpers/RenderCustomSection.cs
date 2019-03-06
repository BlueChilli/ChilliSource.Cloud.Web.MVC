using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

#if NET_4X
using System.Web.Mvc;
using System.Web.WebPages;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Collection of helpers for rendering snippets of code from within partial views which the rendered location of can be controlled from the layout.
    /// </summary>
    public static class CustomScriptsHelper
    {
        private const string _CustomSection = "RenderCustomSection";

        /// <summary>
        /// Register a code template for rendering elsewhere
        /// </summary>
        /// <param name="html"></param>
        /// <param name="section">section to be registered in for example "scripts"</param>
        /// <param name="template">code template</param>
        /// <returns></returns>
#if NET_4X
        public static HelperResult RegisterCustomSection(this HtmlHelper html, string section, Func<object, HelperResult> template)
#else
        public static HelperResult RegisterCustomSection(this IHtmlHelper html, string section, Func<object, HelperResult> template)
#endif
        {
            return RegisterCustomSection(html, section, Guid.NewGuid(), template);
        }

        /// <summary>
        /// Register a code template for rendering elsewhere
        /// </summary>
        /// <param name="html"></param>
        /// <param name="section">section to be registered in for example "scripts"</param>
        /// <param name="templateKey">To register templates that should only be rendered once
        /// <param name="template">code template</param>
        /// <returns></returns>
#if NET_4X
        public static HelperResult RegisterCustomSection(this HtmlHelper html, string section, Guid templateKey, Func<object, HelperResult> template)
#else
        public static HelperResult RegisterCustomSection(this IHtmlHelper html, string section, Guid templateKey, Func<object, HelperResult> template)
#endif
        {
            var sections = html.ViewContext.HttpContext.Items[_CustomSection] as Dictionary<string, Dictionary<Guid, string>>;

            if (sections == null)
            {
                sections = new Dictionary<string, Dictionary<Guid, string>>();
                html.ViewContext.HttpContext.Items.Add(_CustomSection, sections);
            }

            Dictionary<Guid, string> content = null;
            if (sections.ContainsKey(section))
            {
                content = sections[section];
            }
            else
            {
                content = new Dictionary<Guid, string>();
                sections.Add(section, content);
            }

            if (!content.ContainsKey(templateKey))
            {
                var templateResult = template(null);
                string templateStr;
#if NET_4X
                templateStr = templateResult.ToHtmlString();
#else
                using (var textWriter = new StringWriter())
                {
                    templateResult.WriteTo(textWriter, HtmlEncoder.Default);
                    templateStr = textWriter.ToString();
                }
#endif

                content.Add(templateKey, templateStr);
            }

#if NET_4X
            return new HelperResult(writer => { });
#else
            return new HelperResult(writer => Task.CompletedTask);
#endif
        }

        /// <summary>
        /// Shortcut for registering a custom section for scripts. This is the main type of section registered. 
        /// </summary>
        /// <param name="html"></param>
        /// <param name="template">script template</param>
        /// <returns></returns>
#if NET_4X
        public static HelperResult RegisterCustomScripts(this HtmlHelper html, Func<object, HelperResult> template)
#else
        public static HelperResult RegisterCustomScripts(this IHtmlHelper html, Func<object, HelperResult> template)
#endif        
        {
            return RegisterCustomSection(html, "scripts", template);
        }

        /// <summary>
        /// Render all the registered templates for a section. Usually called in the layout page.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="section">section to output for example "scripts"</param>
        /// <returns></returns>
#if NET_4X
        public static IHtmlContent RenderCustomSection(this HtmlHelper html, string section)
#else
        public static IHtmlContent RenderCustomSection(this IHtmlHelper html, string section)
#endif        
        {
            var result = MvcHtmlStringCompatibility.Empty();

            var sections = html.ViewContext.HttpContext.Items[_CustomSection] as Dictionary<string, Dictionary<Guid, string>>;
            if (sections != null)
            {
                if (sections.ContainsKey(section))
                {
                    var content = sections[section];
                    foreach (var item in content)
                    {
                        result = result.AppendLine(item.Value);
                    }
                }
            }
            return result;
        }
    }
}