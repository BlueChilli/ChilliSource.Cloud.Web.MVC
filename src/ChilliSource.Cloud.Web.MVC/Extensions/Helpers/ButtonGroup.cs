using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using EnumHelper = ChilliSource.Core.Extensions.EnumHelper;
#if NET_4X
using System.Web.Mvc;
using System.Web.Mvc.Html;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.DataProtection;
#endif
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for a group of buttons for enumeration values.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The property of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="selectList">A collection of System.Web.Mvc.SelectList.</param>
        /// <returns>An HTML string for a group of buttons for enumeration values.</returns>
        /// <remarks>In almost all cases consume this function via FieldFor or FieldInnerFor and place a ButtonGroupAttribute on your property.</remarks>
#if NET_4X
        public static IHtmlContent ButtonGroupForEnum<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null, IEnumerable<SelectListItem> selectList = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            object model = metadata.Model;
#else
        public static IHtmlContent ButtonGroupForEnum<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null, IEnumerable<SelectListItem> selectList = null)
        {
#if NETSTANDARD2_0

            var explorer = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
#else
            var expressionProvider = new ModelExpressionProvider(html.MetadataProvider);
            var explorer = expressionProvider.CreateModelExpression(html.ViewData, expression).ModelExplorer;
#endif
            ModelMetadata metadata = explorer.Metadata;
            object model = explorer.Model;
#endif

            var list = selectList;
            if (selectList == null)
            {
                Type enumType = Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType;
                var values = Enum.GetNames(enumType).ToList();
                var names = EnumHelper.GetDescriptions(enumType).ToList();
                list = values.ToSelectList(names);
                var items = RemoveItemAttribute.Resolve(metadata, list.ToList());
                list = new SelectList(items, "Value", "Text");
            }

            return MakeButtonGroup(html, expression, htmlAttributes, metadata, model, list.ToSelectList());
        }

        /// <summary>
        /// Returns HTML string for a group of buttons for Boolean value.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The property of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="trueText">Text for the true value.</param>
        /// <param name="falseText">Text for the false value.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <returns>An HTML string for a group of buttons for Boolean value.</returns>
#if NET_4X
        public static IHtmlContent ButtonGroupForBool<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string trueText = null, string falseText = null, object htmlAttributes = null)
#else
        public static IHtmlContent ButtonGroupForBool<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string trueText = null, string falseText = null, object htmlAttributes = null)
#endif        
        {
            trueText = StringExtensions.DefaultTo(trueText, bool.TrueString);
            falseText = StringExtensions.DefaultTo(falseText, bool.FalseString);
            var names = new string[] { trueText, falseText };
            var values = new string[] { bool.TrueString, bool.FalseString };
            var list = values.ToSelectList(names);

#if NET_4X
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            object model = metadata.Model;
#else
#if NETSTANDARD2_0

            var explorer = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
#else
            var expressionProvider = new ModelExpressionProvider(html.MetadataProvider);
            var explorer = expressionProvider.CreateModelExpression(html.ViewData, expression).ModelExplorer;
#endif
            ModelMetadata metadata = explorer.Metadata;
            object model = explorer.Model;
#endif

            return MakeButtonGroup(html, expression, htmlAttributes, metadata, model, list);
        }

        //todo process htmlAttributes

#if NET_4X
        private static IHtmlContent MakeButtonGroup<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes, ModelMetadata metaData, object model, SelectList list)
#else
        private static IHtmlContent MakeButtonGroup<TModel, TProperty>(IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes, ModelMetadata metaData, object model, SelectList list)
#endif        
        {
            var propertyName = htmlHelper.NameFor(expression).ToString();
            var properyId = htmlHelper.IdFor(expression).ToString();

            var result = htmlHelper.HiddenFor(expression).AsHtmlContent();

            //TODO To support flags uses buttons-checkbox
            result = result.Append(@"<div class=""btn-group"" data-toggle=""buttons-radio"">");

            for (var i = 0; i < list.Count(); i++)
            {
                var item = list.ElementAt(i);
                var onclick = $"$('#{properyId}').val($(this).val()).change();";
                var format = @"<button class=""btn{0}"" name=""{1}"" value=""{2}"" data-toggle=""button"" type=""button"" onclick=""{3}"">{4}</button>";

                result = result.Append(String.Format(format, model != null && model.ToString() == item.Value ? " active" : "", propertyName, item.Value, onclick, item.Text));
            }
            result = result.Append("</div>");

            return result;
        }
    }
}