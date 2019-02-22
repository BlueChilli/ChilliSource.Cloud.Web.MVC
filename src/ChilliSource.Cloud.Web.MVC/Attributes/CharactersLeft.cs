#if NET_4X
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Use StringLength in combination with CharactersLeft to show the characters left countdown close to a string input field.
    /// </summary>
    public class CharactersLeftAttribute : Attribute, IMetadataAware
    {
        public const string Key = "CharactersLeft";

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues[Key] = true;
        }
    }
}
#endif
