using ChilliSource.Core.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;

#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
#endif


namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {

        /// <summary>
        /// Get the label text for a model property. Priority is [Label], [DisplayName], property name then, [unsure of applicability] last segment of expression. 
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model property.</param>

#if NET_4X
        public static string GetLabelTextFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
#else
        public static string GetLabelTextFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata metadata = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider).Metadata;
#endif

            string labelText = (metadata.AdditionalValues.SingleOrDefault(m => (string)m.Key == LabelAttribute.Key).Value as string);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            return labelText ?? metadata.DisplayName ?? metadata.PropertyName.ToSentenceCase(true) ?? htmlFieldName.Split('.').Last();
        }
    }
}