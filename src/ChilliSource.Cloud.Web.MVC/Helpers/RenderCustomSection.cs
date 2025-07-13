using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;

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
        public static HelperResult RegisterCustomSection(this IHtmlHelper html, string section, Func<object, HelperResult> template)
        {
            return RegisterCustomSection(html, section, Guid.NewGuid(), template);
        }

        /// <summary>
        /// Register a code template for rendering elsewhere
        /// </summary>
        /// <param name="html"></param>
        /// <param name="section">section to be registered in for example "scripts"</param>
        /// <param name="templateKey">To register templates that should only be rendered once</param>
        /// <param name="template">code template</param>
        /// <returns></returns>
        public static HelperResult RegisterCustomSection(this IHtmlHelper html, string section, Guid templateKey, Func<object, HelperResult> template)
        {
            var context = html.ViewContext;

            var content = template(null).AsHtmlContent();

            RegisterCustomSection(context, section, templateKey, content);

            //var sections = html.ViewContext.HttpContext.Items[_CustomSection] as Dictionary<string, Dictionary<Guid, IHtmlContent>>;

            //if (sections == null)
            //{
            //    sections = new Dictionary<string, Dictionary<Guid, IHtmlContent>>();
            //    html.ViewContext.HttpContext.Items.Add(_CustomSection, sections);
            //}

            //Dictionary<Guid, IHtmlContent> content = null;
            //if (sections.ContainsKey(section))
            //{
            //    content = sections[section];
            //}
            //else
            //{
            //    content = new Dictionary<Guid, IHtmlContent>();
            //    sections.Add(section, content);
            //}

            //if (!content.ContainsKey(templateKey))
            //{
            //    var templateResult = template(null).AsHtmlContent();
            //    content.Add(templateKey, templateResult);
            //}

            return new HelperResult(writer => Task.CompletedTask);
        }

        /// <summary>
        /// Shortcut for registering a custom section for scripts. This is the main type of section registered. 
        /// </summary>
        /// <param name="html"></param>
        /// <param name="template">script template</param>
        /// <returns></returns>
        public static HelperResult RegisterCustomScripts(this IHtmlHelper html, Func<object, HelperResult> template)
        {
            return RegisterCustomSection(html, "scripts", template);
        }

        /// <summary>
        /// Registers custom JavaScript code for inclusion in the specified view context.
        /// </summary>
        /// <remarks>This method allows you to dynamically add custom JavaScript code to a specific
        /// section of the view. The script is associated with the provided <paramref name="templateKey"/> to ensure
        /// proper organization and avoid conflicts.</remarks>
        /// <param name="context">The <see cref="ViewContext"/> representing the current rendering context of the view.</param>
        /// <param name="templateKey">A unique identifier for the template to associate the script with.</param>
        /// <param name="script">The JavaScript code to register. This must be a valid script string.</param>
        public static void RegisterCustomScripts(ViewContext context, Guid templateKey, string script)
        {
            var content = MvcHtmlStringCompatibility.Create(script);
            RegisterCustomSection(context, "scripts", templateKey, content);
        }

        /// <summary>
        /// Registers a custom section for rendering in the specified view context.
        /// </summary>
        /// <remarks>This method associates the provided HTML content with the specified section and
        /// template key. If the section does not already exist, it is created. If the template key is already
        /// registered for the section, the method does not overwrite the existing content.</remarks>
        /// <param name="context">The <see cref="ViewContext"/> representing the current rendering context. This parameter cannot be <see
        /// langword="null"/>.</param>
        /// <param name="section">The name of the section to register. This value is case-sensitive and cannot be <see langword="null"/> or
        /// empty.</param>
        /// <param name="templateKey">A unique identifier for the template associated with the section.</param>
        /// <param name="html">The HTML content to associate with the specified section and template. This parameter cannot be <see
        /// langword="null"/>.</param>
        public static void RegisterCustomSection(ViewContext context, string section, Guid templateKey, IHtmlContent html)
        {
            if (context.HttpContext.Items[_CustomSection] is not Dictionary<string, Dictionary<Guid, IHtmlContent>> sections)
            {
                sections = [];
                context.HttpContext.Items.Add(_CustomSection, sections);
            }

            Dictionary<Guid, IHtmlContent> content;
            if (sections.TryGetValue(section, out Dictionary<Guid, IHtmlContent> value))
            {
                content = value;
            }
            else
            {
                content = [];
                sections.Add(section, content);
            }

            content.TryAdd(templateKey, html);
        }

        /// <summary>
        /// Render all the registered templates for a section. Usually called in the layout page.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="section">section to output for example "scripts"</param>
        /// <returns></returns>
        public static IHtmlContent RenderCustomSection(this IHtmlHelper html, string section)
        {
            var result = MvcHtmlStringCompatibility.Empty();

            var sections = html.ViewContext.HttpContext.Items[_CustomSection] as Dictionary<string, Dictionary<Guid, IHtmlContent>>;
            if (sections != null)
            {
                if (sections.ContainsKey(section))
                {
                    var content = sections[section];
                    foreach (var item in content)
                    {
                        result = result.Append(item.Value).AppendLine();
                    }
                }
            }
            return result;
        }
    }
}