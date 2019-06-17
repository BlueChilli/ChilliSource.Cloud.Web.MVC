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
#endif
            if (fieldOptions == null)
                fieldOptions = new FieldTemplateOptions();

            var templateModel = fieldOptions.CreateFieldInnerTemplateModel(html, expression);

            //Options may have changed, calling GetOptions()
            templateModel = templateModel.GetOptions().ProcessInnerField(templateModel);    

            if (templateModel.HtmlAttributes.ContainsKey("Name"))
            {
                templateModel.Name = templateModel.HtmlAttributes["Name"].ToString();
            }

            var partialPath = templateModel.GetOptions().GetViewPath();
#if NET_4X
            return html.Partial(partialPath, templateModel).AsHtmlContent();
#else
            return html.PartialAsync(partialPath, templateModel);
#endif
        }        
    }
}