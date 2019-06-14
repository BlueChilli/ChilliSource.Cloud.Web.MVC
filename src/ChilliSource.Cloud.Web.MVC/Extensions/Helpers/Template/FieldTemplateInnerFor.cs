using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#if NET_4X
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
#else
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class TemplateHelper
    {
        private static readonly SelectListItem[] SingleEmptyItem = { new SelectListItem { Text = "", Value = "" } };

#if NET_4X
        public static IHtmlContent FieldTemplateInnerFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return FieldTemplateInnerFor(html, expression, new FieldTemplateOptions { HtmlAttributes = htmlAttributes });
        }
#else        
        public static Task<IHtmlContent> FieldTemplateInnerForAsync<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return FieldTemplateInnerForAsync(html, expression, new FieldTemplateOptions { HtmlAttributes = htmlAttributes });
        }
#endif

#if NET_4X
        public static IHtmlContent FieldTemplateInnerFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, FieldTemplateOptionsBase fieldOptions = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            object model = metadata.Model;
#else
        public static Task<IHtmlContent> FieldTemplateInnerForAsync<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, FieldTemplateOptionsBase fieldOptions = null)
        {
            var explorer = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
            ModelMetadata metadata = explorer.Metadata;
            object model = explorer.Model;
#endif
            var member = expression.Body as MemberExpression;

            var httpContext = html.ViewContext.HttpContext;
            var request = httpContext.Request;
            var name = html.NameFor(expression).ToString();

            string attemptedValue = null;
            if (html.ViewContext.ViewData.ModelState.ContainsKey(name))
            {
                var kvp = html.ViewContext.ViewData.ModelState[name];

#if NET_4X
                attemptedValue = kvp.Value?.AttemptedValue;
#else
                attemptedValue = kvp.AttemptedValue;
#endif
            }

            if (fieldOptions == null) fieldOptions = new FieldTemplateOptions();
            var data = new FieldInnerTemplateModel
            {
                Id = html.IdFor(expression).ToString(),
                Name = name,
                Value = String.IsNullOrEmpty(attemptedValue) || (model != null && attemptedValue == model.ToString()) ? model : attemptedValue,
                DisplayName = html.GetLabelTextFor(expression),
                HtmlAttributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(fieldOptions.HtmlAttributes),
                MemberType = typeof(TValue),
                MemberUnderlyingType = metadata.IsNullableValueType ? Nullable.GetUnderlyingType(typeof(TValue)) : typeof(TValue)
            }.UseOptions(fieldOptions);

            if (data.HtmlAttributes.ContainsKey("Id"))
            {
                data.Id = data.HtmlAttributes["Id"].ToString();
                data.HtmlAttributes.Remove("Id");
            }

#if NET_4X
            var validationAttributes = new RouteValueDictionary(html.GetUnobtrusiveValidationAttributes(data.Name, metadata));
#else
            var validator = html.ViewContext.HttpContext.RequestServices.GetService<ValidationHtmlAttributeProvider>();
            var validationAttributes = new Dictionary<string, string>();
            validator?.AddAndTrackValidationAttributes(html.ViewContext, explorer, data.Name, validationAttributes);
#endif
            data.HtmlAttributes.Merge(validationAttributes);            

            data = data.GetOptions().PreProcessInnerField(data, html, metadata, model, member);

            //Options may have changed, calling GetOptions again
            data = data.GetOptions().ProcessInnerField(data, html, metadata, model, member);    

            if (data.HtmlAttributes.ContainsKey("Name"))
            {
                data.Name = data.HtmlAttributes["Name"].ToString();
            }

            var partialPath = data.GetOptions().GetViewPath();
#if NET_4X
            return html.Partial(partialPath, data).AsHtmlContent();
#else
            return html.PartialAsync(partialPath, data);
#endif
        }        
    }
}