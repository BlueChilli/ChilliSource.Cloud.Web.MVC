using ChilliSource.Core.Extensions; using ChilliSource.Cloud.Core;
using System;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        //TODO consider replacing this with a template helper, so html code is not in the library.
        /// <summary>
        /// Returns HTML string for the script element of Google map API with key and library parameters from ChilliSource web project configuration file.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <returns>An HTML string for the script element of Google map API.</returns>
        public static MvcHtmlString ScriptGoogleMapApi(this HtmlHelper htmlHelper)
        {
            var libraries = GlobalMVCConfiguration.Instance.GoogleApis?.Libraries;
            var librariesParam = "";

            if (!String.IsNullOrWhiteSpace(GlobalMVCConfiguration.Instance.GoogleApis?.Libraries))
            {
                librariesParam = String.Format("libraries={0}&", libraries);
            }

            return MvcHtmlString.Create($@"<script type=""text/javascript"" src=""//maps.googleapis.com/maps/api/js?{librariesParam}key={GlobalMVCConfiguration.Instance.GoogleApis?.ApiKey}&sensor=false&language=en""></script>");
        }
    }
}