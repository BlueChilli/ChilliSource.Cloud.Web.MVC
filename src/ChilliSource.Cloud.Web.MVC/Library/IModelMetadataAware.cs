#if NET_4X
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    internal static class MetadataAwareCompatibility
    {
        public static IDictionary<string, object> AdditionalValues(this ModelMetadata metadata)
        {
            return metadata.AdditionalValues;
        }
    }
}
#else
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilliSource.Cloud.Web.MVC
{
    internal static class MetadataAwareCompatibility
    {
        public static IDictionary<object, object> AdditionalValues(this DisplayMetadataProviderContext metadata)
        {
            return metadata.DisplayMetadata.AdditionalValues;
        }

        public static IReadOnlyDictionary<object, object> AdditionalValues(this ModelMetadata metadata)
        {
            return metadata.AdditionalValues;
        }
    }

    // see http://blog.emikek.com/reinstating-imetadataaware-in-asp-net-5-vnext-mvc-6/
    public interface IMetadataAware
    {
        void GetDisplayMetadata(DisplayMetadataProviderContext metadata);
    }
}
#endif