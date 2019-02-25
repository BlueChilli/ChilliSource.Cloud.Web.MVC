using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Use StringLength in combination with CharactersLeft to show the characters left countdown close to a string input field.
    /// </summary>
    public class CharactersLeftAttribute : Attribute, IMetadataAware
    {
        public const string Key = "CharactersLeft";

#if NET_4X
        public void OnMetadataCreated(ModelMetadata metadata)
#else
        public void GetDisplayMetadata(DisplayMetadataProviderContext metadata)
#endif        
        {
            metadata.AdditionalValues()[Key] = true;
        }
    }
}
