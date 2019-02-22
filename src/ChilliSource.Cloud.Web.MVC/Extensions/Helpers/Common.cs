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
        /// Returns HTML string using the specified text when condition is true.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="condition">True to use the specified text, otherwise not.</param>
        /// <param name="result">The specified text.</param>
        /// <returns>An HTML string using the specified text</returns>
        public static MvcHtmlString When(this HtmlHelper htmlHelper, bool condition, string result)
        {            
            return condition ? MvcHtmlStringCompatibility.Create(result) : MvcHtmlStringCompatibility.Empty();
        }

        /// <summary>
        /// Get the model state value of a model property (attempted value will override model value)
        /// </summary>
        /// <returns>Value of prop taking into account postback value (attempted value)</returns>
        public static TValue GetModelStateValue<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var name = html.NameFor(expression).ToString();
#if NET_4X
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            object model = metadata.Model;
#else
            var explorer = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
            ModelMetadata metadata = explorer.Metadata;
            object model = explorer.Model;
#endif

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