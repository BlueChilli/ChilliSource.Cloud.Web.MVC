using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

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
        public static IHtmlContent When(this IHtmlHelper htmlHelper, bool condition, string result)
        {
            return condition ? MvcHtmlStringCompatibility.Create(result) : MvcHtmlStringCompatibility.Empty();
        }

        /// <summary>
        /// Get the model state value of a model property (attempted value will override model value)
        /// </summary>
        /// <returns>Value of prop taking into account postback value (attempted value)</returns>

        public static TValue GetModelStateValue<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var expressionProvider = new ModelExpressionProvider(html.MetadataProvider);
            var explorer = expressionProvider.CreateModelExpression(html.ViewData, expression).ModelExplorer;
            object model = explorer.Model;
            var name = html.NameFor(expression).ToString();

            string? attemptedValue = null;
            if (html.ViewContext.ViewData.ModelState.ContainsKey(name))
            {
                var kvp = html.ViewContext.ViewData.ModelState[name];
                attemptedValue = kvp.AttemptedValue;
            }
            var result = String.IsNullOrEmpty(attemptedValue) || (model != null && attemptedValue == model.ToString()) ? model : attemptedValue;
            if (result == null) return default;

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
            if (value is bool v) return v;
            if (value is string s)
            {
                if (bool.TryParse(s.Split(',')[0], out bool result)) return result;
                return false;
            }
            return false;
        }
    }
}