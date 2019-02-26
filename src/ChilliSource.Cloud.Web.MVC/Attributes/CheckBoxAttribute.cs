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
    /// Marks a field to be generated as CheckBoxes. The field type must be Enum.
    /// Marks a boolean property to be generated as a checkbox template control
    /// </summary>
    public class CheckBoxAttribute : Attribute, IMetadataAware
    {
        /// <summary>
        /// Use the alternative checkbox template
        /// </summary>
        public bool IsAlternative { get; set; }

        /// <summary>
        /// Optionally display a label for this checkbox
        /// </summary>
        public string Label { get; set; }

#if NET_4X
        public void OnMetadataCreated(ModelMetadata metadata)
#else
        public void GetDisplayMetadata(DisplayMetadataProviderContext metadata)
#endif
        {
            metadata.AdditionalValues()["CheckBox"] = true;
        }
    }
}