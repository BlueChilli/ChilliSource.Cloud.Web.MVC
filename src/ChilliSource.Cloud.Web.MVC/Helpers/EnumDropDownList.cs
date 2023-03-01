﻿using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using EnumHelper = ChilliSource.Core.Extensions.EnumHelper;

#if NET_4X
using System.Web.Mvc;
using System.Web.Mvc.Html;
#else
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;
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
        /// Returns HTML string for a drop down list for enumeration values.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TEnum">The type of the enumeration values.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <returns>An HTML string for a drop down list for enumeration values.</returns>
#if NET_4X
        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression)
#else
        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression)
#endif
        {
            return EnumDropDownListFor(htmlHelper, expression, null);
        }

        /// <summary>
        /// Returns HTML string for a drop down list for enumeration values with HTML attributes specified.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TEnum">The type of the enumeration values.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <returns>An HTML string for a drop down list for enumeration values.</returns>        
#if NET_4X
        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            object model = metadata.Model;
#else
        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
#if NETSTANDARD2_0

            var explorer = ExpressionMetadataProvider.FromLambdaExpression(expression, htmlHelper.ViewData, htmlHelper.MetadataProvider);
#else
            var expressionProvider = new ModelExpressionProvider(htmlHelper.MetadataProvider);
            var explorer = expressionProvider.CreateModelExpression(htmlHelper.ViewData, expression).ModelExplorer;
#endif
            ModelMetadata metadata = explorer.Metadata;
            object model = explorer.Model;
#endif

            Type enumType = Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType;
            var values = EnumHelper.GetValues(enumType).Cast<Enum>();
            var modelValues = model == null ? new string[0] : model.ToString().Split(',');
            for (var i = 0; i < modelValues.Count(); i++) modelValues[i] = modelValues[i].Trim();

            IList<SelectListItem> items = (from value in values
                                           select new SelectListItem
                                           {
                                               Text = value.GetDescription(),
                                               Value = value.ToString(),
                                               Selected = modelValues.Contains(value.ToString())
                                           }).ToList();

            items = EmptyItemAttribute.Resolve(metadata, items, SingleEmptyItem);
            items = RemoveItemAttribute.Resolve(metadata, items);

            // add conditional display attributes
            var objects = htmlAttributes as IDictionary<string, object>;
            var attributes = objects ?? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            if (metadata.AdditionalValues.ContainsKey("ConditionalDisplayPropertyName"))
            {
                attributes["data-conditional-on"] = metadata.AdditionalValues()["ConditionalDisplayPropertyName"];
                attributes["data-conditional-values"] = string.Join(",", (object[])metadata.AdditionalValues()["ConditionalDisplayPropertyValues"]);
            }

            return htmlHelper.CustomDropDownListFor(expression, items.ToList(), attributes);
        }

        /// <summary>
        /// Returns an HTML select element for each property in the object that is represented by the specified expression using the specified collection items and HTML attributes.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TEnum">The type of the property.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="values">A collection of System.Collections.Generic.IEnumerable&lt;string&gt; objects that are used to populate the drop-down list.</param>
        /// <param name="expression">An expression that identifies the object that contains the properties to display.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        /// <returns>An HTML select element.</returns>        
#if NET_4X
        public static IHtmlContent StringArrayListBoxFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, IEnumerable<string> values, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            object model = metadata.Model;
#else
        public static IHtmlContent StringArrayListBoxFor<TModel, TEnum>(this IHtmlHelper<TModel> htmlHelper, IEnumerable<string> values, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
#if NETSTANDARD2_0

            var explorer = ExpressionMetadataProvider.FromLambdaExpression(expression, htmlHelper.ViewData, htmlHelper.MetadataProvider);
#else
            var expressionProvider = new ModelExpressionProvider(htmlHelper.MetadataProvider);
            var explorer = expressionProvider.CreateModelExpression(htmlHelper.ViewData, expression).ModelExplorer;
#endif
            ModelMetadata metadata = explorer.Metadata;
            object model = explorer.Model;
#endif

            IEnumerable<SelectListItem> items = from value in values
                                                select new SelectListItem
                                                {
                                                    Text = value,
                                                    Value = value,
                                                    Selected = value.Equals(model)
                                                };

            // If the enum is nullable, add an 'empty' item to the collection
            if (metadata.IsNullableValueType)
                items = SingleEmptyItem.Concat(items);

            // add conditional display attributes
            var objects = htmlAttributes as IDictionary<string, object>;
            var attributes = objects ?? HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            if (metadata.AdditionalValues.ContainsKey("ConditionalDisplayPropertyName"))
            {
                attributes["data-conditional-on"] = metadata.AdditionalValues()["ConditionalDisplayPropertyName"];
                attributes["data-conditional-values"] = string.Join(",", (object[])metadata.AdditionalValues()["ConditionalDisplayPropertyValues"]);
            }

            return htmlHelper.ListBoxFor(expression, items, attributes).AsHtmlContent();
        }

#if NET_4X
        private static IHtmlContent CustomDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IList<SelectListItem> selectList, IDictionary<string, object> htmlAttributes)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            object model = metadata.Model;            
#else
        private static IHtmlContent CustomDropDownListFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IList<SelectListItem> selectList, IDictionary<string, object> htmlAttributes)
        {
#if NETSTANDARD2_0

            var explorer = ExpressionMetadataProvider.FromLambdaExpression(expression, htmlHelper.ViewData, htmlHelper.MetadataProvider);
#else
            var expressionProvider = new ModelExpressionProvider(htmlHelper.MetadataProvider);
            var explorer = expressionProvider.CreateModelExpression(htmlHelper.ViewData, expression).ModelExplorer;
#endif
            ModelMetadata metadata = explorer.Metadata;
            object model = explorer.Model;
#endif
            var propertyId = htmlHelper.IdFor(expression).ToString();
            var propertyName = htmlHelper.NameFor(expression).ToString();

#if NET_4X
            var validationAttributes = htmlHelper.GetUnobtrusiveValidationAttributes(propertyName, metadata);
#else
            var validator = htmlHelper.ViewContext.HttpContext.RequestServices.GetService<ValidationHtmlAttributeProvider>();
            var validationAttributes = new Dictionary<string, string>();
            validator?.AddAndTrackValidationAttributes(htmlHelper.ViewContext, explorer, propertyName, validationAttributes);
#endif

            var wrapper = new TagBuilder("div");
            wrapper.AddCssClass("styled-select");

            var select = new TagBuilder("select");
            select.MergeAttribute("id", propertyId);
            select.MergeAttribute("name", propertyName);
            select.MergeAttributes(htmlAttributes);
            select.MergeAttributes(validationAttributes, replaceExisting: false);

            //Turn list into multiple if has flags set
            var type = metadata.IsNullableValueType ? Nullable.GetUnderlyingType(metadata.ModelType) : metadata.ModelType;
            var flags = type.GetCustomAttribute<FlagsAttribute>();
            if (flags != null) select.MergeAttribute("multiple", "multiple");

            var options = MvcHtmlStringCompatibility.Empty();
            foreach (var item in selectList)
            {
                var itemIndex = selectList.IndexOf(item);
                var previousGroup = itemIndex == 0 || (selectList.ElementAt(itemIndex - 1).Group == null) ? "" : selectList.ElementAt(itemIndex - 1).Group.Name;
                if (item.Group != null && item.Group.Name != previousGroup)
                {
                    if (!String.IsNullOrEmpty(previousGroup))
                    {
                        options = options.Append(@"</optgroup>");
                    }
                    options = options.Append($@"<optgroup label=""{item.Group.Name}"">");
                }

                var option = new TagBuilder("option");
                option.SetInnerText(item.Text);
                option.MergeAttribute("value", item.Value);

                if (item.Selected || (model != null && item.Value.Equals(model.ToString())))
                    option.MergeAttribute("selected", "selected");
                options = options.Append(option.AsHtmlContent(TagRenderMode.Normal));

                if ((item.Group != null && itemIndex == selectList.Count() - 1) ||
                    (!String.IsNullOrEmpty(previousGroup) && (item.Group == null || itemIndex == selectList.Count() - 1)))
                {
                    options = options.Append(@"</optgroup>");
                }
            }

            select.SetInnerHtml(options);
            wrapper.SetInnerHtml(select.AsHtmlContent(TagRenderMode.Normal)
                                       .Append(@"<div class=""arrow""></div>"));

            return wrapper.AsHtmlContent(TagRenderMode.Normal);
        }

        private static readonly SelectListItem[] SingleEmptyItem = { new SelectListItem { Text = "", Value = "" } };

    }
}