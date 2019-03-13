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
using System.Linq;
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

    // Modified from http://blog.emikek.com/reinstating-imetadataaware-in-asp-net-5-vnext-mvc-6/
    public interface IMetadataAware
    {
        void GetDisplayMetadata(DisplayMetadataProviderContext metadata);
    }

    // Modified from http://blog.emikek.com/reinstating-imetadataaware-in-asp-net-5-vnext-mvc-6/
    public class MetadataAwareProvider : IDisplayMetadataProvider
    {
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            if (context == null || context.Attributes.Count == 0)
            {
                return;
            }

            foreach (var att in context.Attributes.OfType<IMetadataAware>())
            {
                att.GetDisplayMetadata(context);
            }
        }
    }
}
#endif