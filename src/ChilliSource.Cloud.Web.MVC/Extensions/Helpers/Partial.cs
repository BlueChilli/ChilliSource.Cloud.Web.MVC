using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NET_4X
using System.Web.Mvc;
using System.Web.Mvc.Html;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
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
#if NET_4X
        public static IHtmlContent PartialFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, string partialViewName)
        {
            string name = ExpressionHelper.GetExpressionText(expression);
            object model = ModelMetadata.FromLambdaExpression(expression, helper.ViewData).Model;

            string htmlFieldPrefix;
            if (helper.ViewData.TemplateInfo.HtmlFieldPrefix != "")
            {
                htmlFieldPrefix = $"{helper.ViewData.TemplateInfo.HtmlFieldPrefix}{(name == "" ? "" : "." + name)}";
            }
            else
            {
                htmlFieldPrefix = name;
            }

            var viewData = new ViewDataDictionary(helper.ViewData)
            {
                TemplateInfo = new System.Web.Mvc.TemplateInfo
                {
                    HtmlFieldPrefix = htmlFieldPrefix
                }
            };

            return helper.Partial(partialViewName, model, viewData).AsHtmlContent();
        }
#else
        public static Task<IHtmlContent> PartialForAsync<TModel, TProperty>(this IHtmlHelper<TModel> html, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, string partialViewName)
        {
#if NETSTANDARD2_0

            var explorer = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
#else
            var expressionProvider = new ModelExpressionProvider(html.MetadataProvider);
            var explorer = expressionProvider.CreateModelExpression(html.ViewData, expression).ModelExplorer;
#endif
#if NETCOREAPP
            string htmlFieldName = expressionProvider.GetExpressionText(expression);
#else
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
#endif
            object model = explorer.Model;

            string htmlFieldPrefix;
            if (html.ViewData.TemplateInfo.HtmlFieldPrefix != "")
            {
                htmlFieldPrefix = $"{html.ViewData.TemplateInfo.HtmlFieldPrefix}{(htmlFieldName == "" ? "" : "." + htmlFieldName)}";
            }
            else
            {
                htmlFieldPrefix = htmlFieldName;
            }

            var viewData = new ViewDataDictionary(html.ViewData);
            viewData.TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;

            return html.PartialAsync(partialViewName, model, viewData);
        }
#endif
    }
}
