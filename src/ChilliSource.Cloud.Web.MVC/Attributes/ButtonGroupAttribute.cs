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
    /// Renders a Bool or Enum property as a group of buttons
    /// </summary>
    public class ButtonGroupAttribute : Attribute, IMetadataAware
    {
        public string FalseText { get; set; }
        public string TrueText { get; set; }

        public ButtonGroupAttribute()
        {
            TrueText = "Yes";
            FalseText = "No";
        }

        public ButtonGroupAttribute(string falseText, string trueText)
        {
            FalseText = falseText;
            TrueText = trueText;
        }

#if NET_4X
        public void OnMetadataCreated(ModelMetadata metadata)
#else
        public void GetDisplayMetadata(DisplayMetadataProviderContext metadata)
#endif
        {
            metadata.AdditionalValues()["ButtonGroup"] = true;
            if (!String.IsNullOrEmpty(FalseText)) metadata.AdditionalValues()["ButtonGroupFalseText"] = FalseText;
            if (!String.IsNullOrEmpty(FalseText)) metadata.AdditionalValues()["ButtonGroupTrueText"] = TrueText;
        }
    }
}