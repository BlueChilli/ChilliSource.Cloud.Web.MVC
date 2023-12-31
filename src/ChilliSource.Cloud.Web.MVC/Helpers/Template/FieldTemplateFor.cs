using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
#if NET_4X
        private static FieldTemplateContent CreateTemplateContent<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options = null)
#else
        private static Task<FieldTemplateContent> CreateTemplateContentAsync<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options = null)
#endif        
        {
            if (options == null)
                options = new TemplateOptions();

            var member = expression.Body as MemberExpression;
            var isMandatory = member.Member.GetAttribute<RequiredAttribute>(false) != null;

            var helpTextAttribute = member.Member.GetAttribute<HelpTextAttribute>(false);
            if (String.IsNullOrEmpty(options.HelpText))
            {
                options.HelpText = helpTextAttribute == null ? "" : helpTextAttribute.Value;
            }

            var data = new FieldTemplateModel
            {
                ModelId = html.IdFor(expression).ToString(),
                ModelName = html.NameFor(expression).ToString(),
                DisplayName = options.Label ?? html.GetLabelTextFor(expression),
                IsMandatory = options.IsMandatory ?? isMandatory,
                HelpText = options.HelpText,
                FieldColumnSize = options.FieldColumnSize,
                FieldSize = options.FieldSize,
                HtmlAttributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(options.HtmlAttributes)
            };

#if NET_4X
            return CreateTemplateContent(html, options.Template, data);
#else
            return CreateTemplateContentAsync(html, options.Template, data);
#endif
        }

#if NET_4X
        public static IHtmlContent FieldTemplateFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IFieldTemplateLayout template)
        {
            return FieldTemplateFor(html, expression, new TemplateOptions { Template = template }, TemplateOptions.CreateDefaultFieldTemplateOptions());
        }

        public static IHtmlContent FieldTemplateFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options = null, IFieldTemplateOptions fieldOptions = null)
        {
            if (options == null) options = new TemplateOptions();
            if (fieldOptions == null) fieldOptions = TemplateOptions.CreateDefaultFieldTemplateOptions();

            var templateContent = CreateTemplateContent(html, expression, options);
            var templateInner = html.FieldTemplateInnerFor<TModel, TValue>(expression, fieldOptions);

            return new FieldTemplateHtmlContent(templateContent, templateInner);
        }

        public static IDisposable FieldTemplateOuterFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IFieldTemplateLayout template)
        {
            return FieldTemplateOuterFor(html, expression, new TemplateOptions { Template = template });
        }

        public static IDisposable FieldTemplateOuterFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options = null)
        {
            var templateContent = CreateTemplateContent(html, expression, options);

            return new DisposableWrapper(
             () => html.ViewContext.Writer.Write(templateContent.BeginContent().ToHtmlString()),
             () => html.ViewContext.Writer.Write(templateContent.EndContent().ToHtmlString()));
        }

        private static IHtmlContent FieldTemplateOuterForBegin<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options)
        {
            return CreateTemplateContent(html, expression, options).BeginContent();
        }

        private static IHtmlContent FieldTemplateOuterForEnd<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options)
        {
            return CreateTemplateContent(html, expression, options).EndContent();
        }
#else
        public static Task<IHtmlContent> FieldTemplateForAsync<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IFieldTemplateLayout template)
        {
            return FieldTemplateForAsync(html, expression, new TemplateOptions { Template = template }, TemplateOptions.CreateDefaultFieldTemplateOptions());
        }

        public static async Task<IHtmlContent> FieldTemplateForAsync<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options = null, IFieldTemplateOptions fieldOptions = null)
        {
            if (options == null) options = new TemplateOptions();
            if (fieldOptions == null) fieldOptions = TemplateOptions.CreateDefaultFieldTemplateOptions();

            var templateContent = await CreateTemplateContentAsync(html, expression, options);
            var templateInner = await html.FieldTemplateInnerForAsync<TModel, TValue>(expression, fieldOptions);

            return new FieldTemplateHtmlContent(templateContent, templateInner);
        }

        public static Task<IDisposable> FieldTemplateOuterForAsync<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IFieldTemplateLayout template)
        {
            return FieldTemplateOuterForAsync(html, expression, new TemplateOptions { Template = template });
        }

        public static async Task<IDisposable> FieldTemplateOuterForAsync<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options = null)
        {
            var templateContent = await CreateTemplateContentAsync(html, expression, options);

            return new DisposableWrapper(html, () => templateContent.BeginContent(), () => templateContent.EndContent());
        }

        private static async Task<IHtmlContent> FieldTemplateOuterForBeginAsync<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options)
        {
            return (await CreateTemplateContentAsync(html, expression, options)).BeginContent();
        }

        private static async Task<IHtmlContent> FieldTemplateOuterForEndAsync<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options)
        {
            return (await CreateTemplateContentAsync(html, expression, options)).EndContent();
        }
#endif
    }
}
