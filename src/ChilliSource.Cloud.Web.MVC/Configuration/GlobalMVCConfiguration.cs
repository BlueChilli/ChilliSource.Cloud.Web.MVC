
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

        private GlobalMVCConfiguration() { }
    }
}
