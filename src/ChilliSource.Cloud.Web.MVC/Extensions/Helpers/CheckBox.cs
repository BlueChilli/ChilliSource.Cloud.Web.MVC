
using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;
using System.Web;

#if NET_4X
using System.Web.Mvc;
using System.Web.Mvc.Html;
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
        /// Returns HTML string for checkboxes for enumeration values with flag attribute.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The property of the value.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <returns>An HTML string for checkboxes for enumeration values with flag attribute.</returns>
        [Obsolete("no field template replace as of yet")]
        public static MvcHtmlString CheckBoxForFlagEnum<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
        {
#if NET_4X
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
#else
            ModelMetadata metadata = ExpressionMetadataProvider.FromLambdaExpression(expression, htmlHelper.ViewData, htmlHelper.MetadataProvider).Metadata;
#endif
            Type enumModelType = metadata.ModelType;

            // Check to make sure this is an enum.
            if (!enumModelType.IsEnum)
            {
                throw new ArgumentException("This helper can only be used with enums. Type used was: " + enumModelType.FullName.ToString() + ".");
            }

            // add conditional display attributes
            var attributes = htmlAttributes is IDictionary<string, object> ? (IDictionary<string, object>)htmlAttributes : (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            // Create string for Element.
            var sb = new StringBuilder();
            foreach (Enum item in Enum.GetValues(enumModelType))
            {
                if (Convert.ToInt32(item) != 0)
                {
                    var propertyName = htmlHelper.NameFor(expression).ToString();
                    var id = string.Format(
                    "{0}{1}_{2}",
                    String.IsNullOrEmpty(htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix) ? "" : htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix + "_",
                    propertyName,
                    item.ToString()
                    );

                    //Add Label
                    var label = new TagBuilder("label");
                    label.Attributes["for"] = id;
                    label.SetInnerText(item.GetDescription());

                    // Add checkbox.
                    var checkbox = new TagBuilder("input");
                    checkbox.Attributes["id"] = id;
                    checkbox.Attributes["name"] = propertyName;
                    checkbox.Attributes["type"] = "checkbox";
                    checkbox.Attributes["value"] = item.ToString();
                    checkbox.Attributes["class"] = propertyName;
                    checkbox.MergeAttributes(attributes);

                    var model = metadata.Model as Enum;
                    long targetValue = Convert.ToInt64(item);
                    long flagValue = Convert.ToInt64(model);

                    if ((targetValue & flagValue) == targetValue)
                        checkbox.Attributes["checked"] = "checked";

                    sb.AppendFormat
                        (
                            @"<div>{0}{1}</div>",
                            checkbox.ToString(),
                            label.ToString()
                        );
                }
            }

            return MvcHtmlStringCompatibility.Create(sb.ToString());
        }
    }
}