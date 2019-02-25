using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Sets a custom label for a field.
    /// </summary>
    public class LabelAttribute : Attribute, IMetadataAware
    {
        public const string Key = "CharactersLeft";

        /// <summary>
        /// Label text
        /// </summary>
        public string Value { get; set; }

        public LabelAttribute(string value)
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
    }
}