#if NET_4X
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class TemplateHelper
    {
        public static IHtmlContent FieldTemplateFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateType template)
        {
            return FieldTemplateFor(html, expression, new TemplateOptions { Template = template }, new FieldTemplateOptions());
        }

        public static IHtmlContent FieldTemplateFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options = null, FieldTemplateOptions fieldOptions = null)
        {
            if (options == null) options = new TemplateOptions();
            if (fieldOptions == null) fieldOptions = new FieldTemplateOptions();

            var templateStart = html.FieldTemplateOuterForBegin<TModel, TValue>(expression, options);
            var templateInner = html.FieldTemplateInnerFor<TModel, TValue>(expression, fieldOptions);
            var templateEnd = html.FieldTemplateOuterForEnd<TModel, TValue>(expression, options);
            var template = MvcHtmlStringCompatibility.Empty().Format("{0}{1}{2}", templateStart, templateInner, templateEnd);
            return template;
        }

        public static IDisposable FieldTemplateOuterFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateType template)
        {
            return FieldTemplateOuterFor(html, expression, new TemplateOptions { Template = template });
        }

        public static IDisposable FieldTemplateOuterFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options = null)
        {
            if (options == null) options = new TemplateOptions();
            return new DisposableWrapper(
                () => html.ViewContext.Writer.Write(html.FieldTemplateOuterForBegin<TModel, TValue>(expression, options)),
                () => html.ViewContext.Writer.Write(html.FieldTemplateOuterForEnd<TModel, TValue>(expression, options))
            );

        }

        private static IHtmlContent FieldTemplateOuterForBegin<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options)
        {
            var member = expression.Body as MemberExpression;
            var isMandatory = member.Member.GetAttribute<RequiredAttribute>(false) != null;

            var helpTextAttribute = member.Member.GetAttribute<HelpTextAttribute>(false);
            if (String.IsNullOrEmpty(options.HelpText))
            {
                options.HelpText = helpTextAttribute == null ? "" : helpTextAttribute.Value;
            }

            var data = new FieldTemplateModel
            {
                ModelName = html.NameFor(expression).ToString(),
                DisplayName = options.Label ?? html.GetLabelTextFor(expression),
                IsMandatory = options.IsMandatory ?? isMandatory,
                HelpText = options.HelpText,
                FieldColumnSize = options.FieldColumnSize,
                FieldSize = options.FieldSize
            };

            return ContainerTemplateBegin(html, options.Template.ToString(), data, "FieldTemplates");
        }

        private static IHtmlContent FieldTemplateOuterForEnd<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TemplateOptions options)
        {
            var data = new FieldTemplateModel
            {
                ModelName = html.NameFor(expression).ToString(),
                HelpText = options.HelpText
            };

            return ContainerTemplateEnd(html, options.Template.ToString(), data, "FieldTemplates");
        }

    }
}
#endif