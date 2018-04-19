using ChilliSource.Core.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;


namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {

        /// <summary>
        /// Get the label text for a model property. Priority is [Label], [DisplayName], property name then, [unsure of applicability] last segment of expression. 
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model property.</param>
        public static string GetLabelTextFor<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string labelText = (metadata.AdditionalValues.SingleOrDefault(m => m.Key == LabelAttribute.Key).Value as string);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            return labelText ?? metadata.DisplayName ?? metadata.PropertyName.ToSentenceCase(true) ?? htmlFieldName.Split('.').Last();
        }
    }
}