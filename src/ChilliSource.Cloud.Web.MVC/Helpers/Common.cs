using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// <summary>
        /// Returns HTML string using the specified text when condition is true.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="condition">True to use the specified text, otherwise not.</param>
        /// <param name="result">The specified text.</param>
        /// <returns>An HTML string using the specified text</returns>
#if NET_4X
        public static IHtmlContent When(this HtmlHelper htmlHelper, bool condition, string result)
#else
        public static IHtmlContent When(this IHtmlHelper htmlHelper, bool condition, string result)
#endif
        {
            return condition ? MvcHtmlStringCompatibility.Create(result) : MvcHtmlStringCompatibility.Empty();
        }

        /// <summary>
        /// Get the model state value of a model property (attempted value will override model value)
        /// </summary>
        /// <returns>Value of prop taking into account postback value (attempted value)</returns>

#if NET_4X
        public static TValue GetModelStateValue<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {            
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            object model = metadata.Model;
#else
        public static TValue GetModelStateValue<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
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
            var result = String.IsNullOrEmpty(attemptedValue) || (model != null && attemptedValue == model.ToString()) ? model : attemptedValue;
            if (result == null) return default(TValue);

            Type t = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
            if (t.IsEnum && result is string) return (TValue)Enum.Parse(t, (string)result);

            string typeName = t.Name;
            if (typeName == "Boolean")
            {
                return (TValue)Convert.ChangeType(ConvertAttemptedValueToBoolean(result), t);
            }
            return (TValue)Convert.ChangeType(result, t);
        }

        public static bool ConvertAttemptedValueToBoolean(object value)
        {
            if (value == null) return false;
            if (value is bool) return (bool)value;
            if (value is string)
            {
                var s = value as string;
                return Convert.ToBoolean(s.Split(',')[0]);  // Handle posted values like "true,false"
            }
            return false;
        }
    }
}