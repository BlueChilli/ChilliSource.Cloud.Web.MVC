using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Displays a help text after the input field.
    /// </summary>
    public class HelpTextAttribute : Attribute, IMetadataAware
    {
        /// <summary>
        /// Help text to be displayed
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Specifies whether the help text should be displayed in-line or as a block (under the input field).
        /// </summary>
        public bool DisplayAsBlock { get; set; }

        public HelpTextAttribute(string value, bool displayAsBlock = true)
        {
            Value = value;
            DisplayAsBlock = displayAsBlock;

        }

#if NET_4X
        public void OnMetadataCreated(ModelMetadata metadata)
#else
        public void GetDisplayMetadata(DisplayMetadataProviderContext metadata)
#endif
        {
            metadata.AdditionalValues()["HelpText"] = Value;
            metadata.AdditionalValues()["HelpText-Display"] = (DisplayAsBlock) ? "help-block" : "help-inline";
        }

#if NET_4X
        public static IHtmlContent GetHelpTextFor<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object transformData = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
#else
        public static IHtmlContent GetHelpTextFor<TModel, TValue>(IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object transformData = null)
        {
            var explorer = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
            ModelMetadata metadata = explorer.Metadata;
#endif

            string helpText = (metadata.AdditionalValues.SingleOrDefault(m => (string)m.Key == "HelpText").Value as string);
            if (string.IsNullOrEmpty(helpText)) return MvcHtmlStringCompatibility.Create("");

            string helpTextDisplay = (metadata.AdditionalValues.SingleOrDefault(m => (string)m.Key == "HelpText-Display").Value as string);
            if (transformData != null) helpText = helpText.TransformWith(transformData);

            return MvcHtmlStringCompatibility.Create(string.Format(@"<p class=""{0}"">{1}</p>", helpTextDisplay, helpText));
        }
    }
}