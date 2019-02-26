
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Web.MVC
{
    public class GlobalMVCConfiguration
    {
        private static readonly GlobalMVCConfiguration _instance = new GlobalMVCConfiguration();
        public static GlobalMVCConfiguration Instance { get { return _instance; } }

        private GlobalMVCConfiguration()
        {
        }

        internal GoogleApis GoogleApis { get; set; }        

        public void SetGoogleApisSettings(string apiKey, string libraries)
        {
            GoogleApis = new GoogleApis
            {
                ApiKey = apiKey,
                Libraries = libraries
            };
        }
    }

    internal class GoogleApis
    {
        internal string ApiKey { get; set; }
        internal string Libraries { get; set; }
    }
}
