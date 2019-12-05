using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChilliSource.Cloud.Web;
using ChilliSource.Core.Extensions;

#if NET_4X
using System.Web.Mvc;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Adds placeholder value to an input field. Value defaults to what is displayed as the label (for when label is hidden).
    /// Otherwise pass in the value you want displayed.
    /// </summary>
    public class PlaceholderAttribute : Attribute, IMetadataAware
    {
        public const string Key = "Placeholder";

        /// <summary>
        /// Placeholder text description.
        /// </summary>
        public string Value { get; set; }

        public PlaceholderAttribute(string value = null)
        {
            Value = value;
        }

#if NET_4X
        public void OnMetadataCreated(ModelMetadata metadata)
#else
        public void GetDisplayMetadata(DisplayMetadataProviderContext metadata)
#endif 
        {
            metadata.AdditionalValues()[Key] = Value;
        }

        public static string Resolve(ModelMetadata metadata, IDictionary<string, object> attributes, bool asDataAttribute = false)
        {
            if (metadata.AdditionalValues.ContainsKey(Key))
            {
                var value = metadata.AdditionalValues()[Key] as string;
                var placeholderText = value == null ? metadata.GetDisplayName().SplitByUppercase() : value;
                var attribute = asDataAttribute ? "data-placeholder" : "placeholder";
                attributes.AddOrSkipIfExists(attribute, placeholderText);
                return placeholderText;
            }
            return String.Empty;
        }
    }
}