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
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
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
        public static Task<IHtmlContent> PartialForAsync<TModel, TProperty>(this IHtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, string partialViewName)
        {
            string name = ExpressionHelper.GetExpressionText(expression);
            object model = ExpressionMetadataProvider.FromLambdaExpression(expression, helper.ViewData, helper.MetadataProvider).Model;

            string htmlFieldPrefix;
            if (helper.ViewData.TemplateInfo.HtmlFieldPrefix != "")
            {
                htmlFieldPrefix = $"{helper.ViewData.TemplateInfo.HtmlFieldPrefix}{(name == "" ? "" : "." + name)}";
            }
            else
            {
                htmlFieldPrefix = name;
            }

            var viewData = new ViewDataDictionary(helper.ViewData);
            viewData.TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;

            return helper.PartialAsync(partialViewName, model, viewData);
        }
#endif
    }
}
