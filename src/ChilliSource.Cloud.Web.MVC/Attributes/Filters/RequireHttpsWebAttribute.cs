using System;
#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Forces request to be over https protocol. Works with AWS load balancing
    /// </summary>
    public class RequireHttpsWeb : RequireHttpsAttribute
    {
#if NET_4X
        public override void OnAuthorization(AuthorizationContext filterContext)
#else
        public override void OnAuthorization(AuthorizationFilterContext filterContext)
#endif
        {
            if (IsForwardedSsl(filterContext))
            {
                return;
            }
            base.OnAuthorization(filterContext);
        }

#if NET_4X
        private static bool IsForwardedSsl(AuthorizationContext actionContext)
#else
        private static bool IsForwardedSsl(AuthorizationFilterContext actionContext)
#endif
        {
            var xForwardedProto = actionContext.HttpContext.Request.Headers["X-Forwarded-Proto"];
            var forwardedSsl = !string.IsNullOrWhiteSpace(xForwardedProto) &&
                string.Equals(xForwardedProto, "https", StringComparison.InvariantCultureIgnoreCase);
            return forwardedSsl;
        }
    }
}