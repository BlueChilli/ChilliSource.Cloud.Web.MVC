using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace ChilliSource.Cloud.Web.MVC
{
    public class RadioItemAttribute : Attribute, IMetadataAware
    {

        public RadioItemAttribute()
        {
        }

        public void GetDisplayMetadata(DisplayMetadataProviderContext metadata)
        {
            metadata.DisplayMetadata.AdditionalValues["RadioItem"] = true;
        }
    }
}
