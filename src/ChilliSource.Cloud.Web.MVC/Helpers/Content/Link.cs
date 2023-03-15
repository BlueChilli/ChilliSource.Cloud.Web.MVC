using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using System;
using System.Web;

#if NET_4X
using System.Web.Mvc;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Represents support for rendering HTML link element (&lt;a&gt; tag) in a view.
    /// </summary>
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for the link element.
        /// </summary>
        /// <param name="urlHelper">The System.Web.Mvc.UrlHelper.</param>
        /// <param name="actionName">The name of the action used to generate URL of the link element.</param>
        /// <param name="controllerName">The name of the controller used to generate URL of the link element.</param>
        /// <param name="area">The name of the area used to generate URL of the link element.</param>
        /// <param name="routeName">The name of the route used to generate URL of the link element.</param>
        /// <param name="id">The value of ID in the route values used to generate URL of the link element.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the link element.</param>
        /// <param name="displayText">The text to display for the link element.</param>
        /// <param name="linkClasses">The CSS class for the link element.</param>
        /// <param name="iconClasses">The CSS icon class for the link element.</param>
        /// <param name="linkAttributes">An object that contains the HTML attributes to set for the link element.</param>
        /// <param name="hostName">The host name for the link.</param>
        /// <param name="fragment">A URL fragment identifier (#).</param>
        /// <returns>An HTML-encoded string for the link element.</returns>
#if NET_4X
        public static IHtmlContent Link(UrlHelper urlHelper, string actionName = "", string controllerName = "", string area = null, string routeName = "", string id = "", object routeValues = null, string displayText = "", string linkClasses = "", string iconClasses = "", object linkAttributes = null, string hostName = "", string fragment = "")
#else
        public static IHtmlContent Link(IUrlHelper urlHelper, string actionName = "", string controllerName = "", string area = null, string routeName = "", string id = "", object routeValues = null, string displayText = "", string linkClasses = "", string iconClasses = "", object linkAttributes = null, string hostName = "", string fragment = "")
#endif        
        {
            displayText = String.IsNullOrEmpty(displayText) ? actionName.SplitByUppercase() : displayText;
            TagBuilder tag = new TagBuilder("a");
            var iconHtmlContent = MvcHtmlStringCompatibility.Empty();

            if (!String.IsNullOrEmpty(iconClasses))
            {
                var iconTag = new TagBuilder("i");
                iconTag.AddCssClass(iconClasses);

                iconHtmlContent = iconHtmlContent.Append(iconTag.AsHtmlContent().Append(" "));
            }

            tag.SetInnerHtml(iconHtmlContent.Append(displayText));

            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(linkAttributes);

            tag.MergeAttributes(attributes);
            if (!String.IsNullOrEmpty(linkClasses)) tag.AddCssClass(linkClasses);

            if (attributes["onclick"] == null)
            {
#if NET_4X
                var href = urlHelper.DefaultAction(actionName, controllerName, area, routeName, id, routeValues, hostName: hostName, fragment: fragment);
#else
                var routeDict = new RouteValueDictionary(routeValues);
                if (!String.IsNullOrEmpty(area))
                {
                    routeDict["area"] = area;
                }
                if (!String.IsNullOrEmpty(id))
                {
                    routeDict["id"] = id;
                }

                var href = urlHelper.Action(new UrlActionContext()
                {
                    Action = actionName,
                    Controller = controllerName,
                    Values  = routeDict,
                    Host = hostName,
                    Fragment = fragment,
                    Protocol = urlHelper.ActionContext.HttpContext.Request.Scheme
                });
#endif

                tag.MergeAttribute("href", href);
            }

            return tag.AsHtmlContent();
        }

        /// <summary>
        /// Creates a link to a external site. If protocol is not specified relative protocol is used. If target is not specified in linkAttribtues "_blank" is used.  
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="absoluteUrl">The absolute URL of the link.</param>
        /// <param name="displayText">The text to display for the link element.</param>
        /// <param name="linkClasses">The CSS class for the link element.</param>
        /// <param name="iconClasses">The CSS icon class for the link element.</param>
        /// <param name="linkAttributes">An object that contains the HTML attributes to set for the link element.</param>
        /// <returns>The link or if url is empty or null the displayText is emitted as the result.</returns>
#if NET_4X
        public static IHtmlContent LinkExternal(this HtmlHelper html, string absoluteUrl, string displayText, string linkClasses = "", string iconClasses = "", object linkAttributes = null)
#else
        public static IHtmlContent LinkExternal(this IHtmlHelper html, string absoluteUrl, string displayText, string linkClasses = "", string iconClasses = "", object linkAttributes = null)
#endif        
        {
            if (String.IsNullOrEmpty(absoluteUrl)) return MvcHtmlStringCompatibility.Create(displayText);

            TagBuilder tag = new TagBuilder("a");
            var iconHtmlContent = MvcHtmlStringCompatibility.Empty();

            if (!String.IsNullOrEmpty(iconClasses))
            {
                var iconTag = new TagBuilder("i");
                iconTag.AddCssClass(iconClasses);

                iconHtmlContent = iconHtmlContent.Append(iconTag.AsHtmlContent().Append(" "));
            }

            tag.SetInnerHtml(iconHtmlContent.Append(displayText));

            var attributes = linkAttributes as RouteValueDictionary;
            if (attributes == null) attributes = new RouteValueDictionary(linkAttributes);
            if (!attributes.ContainsKey("target")) attributes.Add("target", "_blank");

            tag.MergeAttributes(attributes);
            if (!String.IsNullOrEmpty(linkClasses)) tag.AddCssClass(linkClasses);

            if (!Uri.IsWellFormedUriString(absoluteUrl, UriKind.Absolute)) absoluteUrl = "//" + absoluteUrl;
            tag.MergeAttribute("href", absoluteUrl);

            return MvcHtmlStringCompatibility.Create(tag, TagRenderMode.Normal);
        }

        /// <summary>
        /// Returns HTML string for the link element to post form.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="actionName">The name of the action used to generate URL of the link element.</param>
        /// <param name="controllerName">The name of the controller used to generate URL of the link element.</param>
        /// <param name="area">The name of the area used to generate URL of the link element.</param>
        /// <param name="routeName">The name of the route used to generate URL of the link element.</param>
        /// <param name="id">The value of ID in the route values used to generate URL of the link element.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the link element.</param>
        /// <param name="displayText">The text to display for the link element.</param>
        /// <param name="linkClasses">The CSS class for the link element.</param>
        /// <param name="iconClasses">The CSS icon class for the link element.</param>
        /// <param name="linkAttributes">An object that contains the HTML attributes to set for the link element.</param>
        /// <param name="confirmFunction">JavaScript function to confirm before from post.</param>
        /// <returns>An HTML-encoded string for the link element to post form.</returns>
        /// <remarks>Uses jquery.doPost.js</remarks>
#if NET_4X
        public static IHtmlContent LinkPost<TModel>(this HtmlHelper<TModel> htmlHelper, string actionName, string controllerName = "", string area = null, string routeName = "", string id = "", object routeValues = null, string displayText = "", string linkClasses = "", string iconClasses = "", object linkAttributes = null, string confirmFunction = "")
#else
        public static IHtmlContent LinkPost<TModel>(this IHtmlHelper<TModel> htmlHelper, string actionName, string controllerName = "", string area = null, string routeName = "", string id = "", object routeValues = null, string displayText = "", string linkClasses = "", string iconClasses = "", object linkAttributes = null, string confirmFunction = "")
#endif        
        {
            var urlHelper = htmlHelper.GetUrlHelper();
            return LinkPost(urlHelper, actionName, controllerName, area, routeName, id, routeValues, displayText, linkClasses, iconClasses, linkAttributes, confirmFunction);
        }

        /// <summary>
        /// Returns HTML string for the link element to post form.
        /// </summary>
        /// <param name="urlHelper">The System.Web.Mvc.UrlHelper.</param>
        /// <param name="actionName">The name of the action used to generate URL of the link element.</param>
        /// <param name="controllerName">The name of the controller used to generate URL of the link element.</param>
        /// <param name="area">The name of the area used to generate URL of the link element.</param>
        /// <param name="routeName">The name of the route used to generate URL of the link element.</param>
        /// <param name="id">The value of ID in the route values used to generate URL of the link element.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the link element.</param>
        /// <param name="displayText">The text to display for the link element.</param>
        /// <param name="linkClasses">The CSS class for the link element.</param>
        /// <param name="iconClasses">The CSS icon class for the link element.</param>
        /// <param name="linkAttributes">An object that contains the HTML attributes to set for the link element.</param>
        /// <param name="confirmFunction">JavaScript function to confirm before from post.</param>
        /// <returns>An HTML-encoded string for the link element to post form.</returns>
        /// <remarks>Uses jquery.doPost.js</remarks>
#if NET_4X
        public static IHtmlContent LinkPost(UrlHelper urlHelper, string actionName = "", string controllerName = "", string area = null, string routeName = "", string id = "", object routeValues = null, string displayText = "", string linkClasses = "", string iconClasses = "", object linkAttributes = null, string confirmFunction = "")
#else
        public static IHtmlContent LinkPost(IUrlHelper urlHelper, string actionName = "", string controllerName = "", string area = null, string routeName = "", string id = "", object routeValues = null, string displayText = "", string linkClasses = "", string iconClasses = "", object linkAttributes = null, string confirmFunction = "")
#endif        
        {
            var href = urlHelper.DefaultAction(actionName, controllerName, area, routeName);

            var data = RouteValueDictionaryExtensions.Create(routeValues);
            if (!String.IsNullOrEmpty(id)) data["id"] = id;

            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(linkAttributes);
            attributes.Add("href", "javascript:void(0);");

            if (String.IsNullOrEmpty(confirmFunction))
            {
                attributes.Add("onclick", String.Format("$.doPost('{0}', '', {1});", href, data.ToJsonString()));
            }
            else
            {
                attributes.Add("onclick", String.Format("if ({0}(this)) $.doPost('{1}', '', {2});", confirmFunction, href, data.ToJsonString()));
            }

            return Link(urlHelper, displayText, linkClasses: linkClasses, iconClasses: iconClasses, linkAttributes: attributes);
        }

        /// <summary>
        /// Returns HTML string for the link element to perform Ajax asynchronous request.
        /// </summary>
        /// <param name="urlHelper">The System.Web.Mvc.UrlHelper.</param>
        /// <param name="target">The ID of the HTML element to be rendered after Ajax asynchronous request succeeded.</param>
        /// <param name="actionName">The name of the action used to generate URL of the link element.</param>
        /// <param name="controllerName">The name of the controller used to generate URL of the link element.</param>
        /// <param name="area">The name of the area used to generate URL of the link element.</param>
        /// <param name="routeName">The name of the route used to generate URL of the link element.</param>
        /// <param name="id">The value of ID in the route values used to generate URL of the link element.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate URL of the link element.</param>
        /// <param name="displayText">The text to display for the link element.</param>
        /// <param name="linkClasses">The CSS class for the link element.</param>
        /// <param name="iconClasses">The CSS icon class for the link element.</param>
        /// <param name="linkAttributes">An object that contains the HTML attributes to set for the link element.</param>
        /// <param name="dynamicData">The data to submit for HTTP request in JSON format.</param>
        /// <param name="customOnAjaxStart">JavaScript function to execute before Ajax asynchronous request.</param>
        /// <param name="callbackJs">JavaScript function to execute after Ajax asynchronous request succeeded.</param>
        /// <returns>An HTML-encoded string for the link element to perform Ajax asynchronous request.</returns>
        public static IHtmlContent LinkAjax(UrlHelper urlHelper, string target, string actionName, string controllerName = "", string area = "", string routeName = "", string id = "", object routeValues = null, string displayText = "", string linkClasses = "", string iconClasses = "", object linkAttributes = null, string dynamicData = "", string customOnAjaxStart = "", string callbackJs = "")
        {
            displayText = displayText.DefaultTo(actionName);
            var href = urlHelper.DefaultAction(actionName, controllerName, area, routeName, id, routeValues);

            var attributes = new RouteValueDictionary(linkAttributes);
            attributes.Add("href", "javascript:void(0);");
            var format = @"$.ajaxLoad('{0}', '{1}', {2}, {4});$.onAjaxStart('{0}');{3}";
            dynamicData = String.IsNullOrEmpty(dynamicData) ? "null" : dynamicData;
            callbackJs = String.IsNullOrEmpty(callbackJs) ? "null" : String.Format("function() {{ {0} }}", callbackJs);
            attributes.Add("onclick", String.Format(format, target, href, dynamicData, customOnAjaxStart, callbackJs));

            return Link(urlHelper, displayText, linkClasses: linkClasses, iconClasses: iconClasses, linkAttributes: attributes);
        }
    }
}