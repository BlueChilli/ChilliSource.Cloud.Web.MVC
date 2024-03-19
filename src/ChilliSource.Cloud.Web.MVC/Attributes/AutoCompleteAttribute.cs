using ChilliSource.Cloud.Web.MVC;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using System;

namespace ChilliSource.Cloud.Web.MVC
{
    public class AutoCompleteAttribute : Attribute, IMetadataAware
    {
        public const string Key = "AutoComplete";
        public bool Value { get; set; }

        public AutoCompleteAttribute(bool value)
        {
            Value = value;
        }

        public void GetDisplayMetadata(DisplayMetadataProviderContext metadata)
        {
            metadata.AdditionalValues()[Key] = Value;
        }

        public static void Resolve(ModelMetadata metadata, RouteValueDictionary attributes)
        {
            if (metadata.AdditionalValues.ContainsKey(Key))
            {
                if (bool.TryParse(metadata.AdditionalValues()[Key] as string, out var value))
                {
                    if (value)
                    {
                        attributes.AddOrSkipIfExists("autocomplete", "off");
                    }
                }
            }
        }
    }
}
