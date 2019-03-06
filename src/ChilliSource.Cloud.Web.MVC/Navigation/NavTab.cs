using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

#if NET_4X
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
#if NET_4X
    public static partial class HtmlHelperExtensions
    {

        //@{
        //    var navTabs = new List<NavTabItem>();
        //    navTabs.Add(new NavTabItem() { Action = "Details", LinkText = "Occupancy", Icon = "icon-list" });
        //    navTabs.Add(new NavTabItem() { Action = "Identity", Icon = "icon-user" });
        //}
        //@Html.NavTabResponsive(navTabs, new NavMenuOptions { RouteValues = new {id = Model.Id} })

        /// <summary>
        /// Returns HTML string for responsive tabs.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="items">The list of NavTabItem.</param>
        /// <param name="menuOptions">Tab options defined by NavMenuOptions</param>
        /// <returns>An HTML string for responsive tabs.</returns>
        public static IHtmlContent NavTabResponsive(this HtmlHelper htmlHelper, List<NavTabItem> items, NavMenuOptions menuOptions)
        {
            menuOptions.NavCssClasses = "visible-desktop";
            var result = NavTabs(htmlHelper, items, menuOptions).AppendLine();

            menuOptions.NavCssClasses = "hidden-desktop";
            result = result.Append(NavPills(htmlHelper, items, menuOptions));

            return result;
        }

        /// <summary>
        /// Returns HTML string for navigation tabs.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="items">The list of NavTabItem.</param>
        /// <param name="menuOptions">Tab options defined by NavMenuOptions</param>
        /// <returns>An HTML string for navigation tabs.</returns>
        public static IHtmlContent NavTabs(this HtmlHelper htmlHelper, List<NavTabItem> items, NavMenuOptions menuOptions = null)
        {
            if (menuOptions == null) menuOptions = new NavMenuOptions();
            NavTabSetDefaults(items, menuOptions);

            var result = MvcHtmlStringCompatibility.Empty();
            result = result.Append(String.Format(@"<ul class=""nav nav-tabs {0}"">", menuOptions.NavCssClasses));

            foreach (var item in items)
            {
                result = result.Append(NavTabItemMaker(htmlHelper, item, menuOptions));
            }
            result = result.Append("</ul>");

            return result;
        }

        /// <summary>
        /// Returns HTML string for navigation pills.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="items">The list of NavTabItem.</param>
        /// <param name="menuOptions">Tab options defined by NavMenuOptions</param>
        /// <returns>An HTML string for navigation pills.</returns>
        public static IHtmlContent NavPills(this HtmlHelper htmlHelper, List<NavTabItem> items, NavMenuOptions menuOptions = null)
        {
            if (menuOptions == null) menuOptions = new NavMenuOptions();
            NavTabSetDefaults(items, menuOptions);

            var result = MvcHtmlStringCompatibility.Empty();
            result = result.Append(String.Format(@"<ul class=""nav nav-pills {0}"">", menuOptions.NavCssClasses));

            foreach (var item in items)
            {
                result = result.Append(NavTabItemMaker(htmlHelper, item, menuOptions, reverseIcon: true));
            }
            result = result.Append("</ul>");

            return result;
        }

        private static void NavTabSetDefaults(List<NavTabItem> items, NavMenuOptions menuOptions)
        {
            foreach (var item in items)
            {
                item.Icon = item.Icon.DefaultTo(menuOptions.Icon);
                item.IconActive = item.IconActive.DefaultTo(menuOptions.IconActive);
                item.RouteValues = RouteValueDictionaryExtensions.Create(item.RouteValues).Merge(RouteValueDictionaryExtensions.Create(menuOptions.RouteValues));
            }
        }

        private static IHtmlContent NavTabItemMaker(this HtmlHelper htmlHelper, NavTabItem item, NavMenuOptions menuOptions, bool reverseIcon = false)
        {
            var urlHelper = htmlHelper.GetUrlHelper();

            var url = urlHelper.DefaultAction(item.Action, item.Controller, item.Area, item.RouteName, null, item.RouteValues);

            if (item.IconActive != null) item.IconActive = item.IconActive.DefaultTo("icon-ok");

            var liTag = new TagBuilder("li");

            bool isActive = item.IsActive;
            if (!isActive) isActive = menuOptions.MatchAction ? urlHelper.IsCurrent(url) : urlHelper.IsCurrent(item.Controller, item.Area);
            if (isActive)
            {
                liTag.AddCssClass("active");
            }

            if (menuOptions.Status != null || !String.IsNullOrEmpty(item.Icon))
            {
                if (menuOptions.Status != null && menuOptions.Status.FirstOrDefault(s => s.Key == (menuOptions.MatchAction ? item.Action : item.Controller)).Value)
                {
                    item.Icon = item.IconActive;
                    if (menuOptions.MatchActiveByStatus)
                    {
                        isActive = true;
                        liTag.AddCssClass("active");
                    }
                }
                if (menuOptions.Status == null && isActive && item.IconActive != null) item.Icon = item.IconActive;
                if (reverseIcon && isActive) item.Icon += " icon-white";
            }

            var anchorTag = MvcHtmlStringCompatibility.Empty();
            if (menuOptions.IsAjax)
            {
                anchorTag = HtmlHelperExtensions.LinkAjax(urlHelper, menuOptions.AjaxTarget, item.Action, item.Controller, item.Area, item.RouteName, "", item.RouteValues, item.LinkText, "", item.Icon, customOnAjaxStart: @"$.onNavTabStart(this);");
            }
            else
            {
                anchorTag = HtmlHelperExtensions.Link(urlHelper, item.Action, item.Controller, item.Area, item.RouteName, "", item.RouteValues, item.LinkText, "", item.Icon);
            }

            liTag.SetInnerHtml(anchorTag);

            return liTag.AsHtmlContent();
        }

        /// <summary>
        /// Returns HTML string for the active navigation tab.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="items">The list of NavTabItem.</param>
        /// <param name="menuOptions">Tab options defined by NavMenuOptions</param>
        /// <returns>An HTML string for the active navigation tab.</returns>
        public static IHtmlContent NavActiveAction(this HtmlHelper htmlHelper, List<NavTabItem> items, NavMenuOptions navOptions = null)
        {
            if (items == null || items.Count == 0) return MvcHtmlStringCompatibility.Empty();
            if (navOptions == null) navOptions = new NavMenuOptions();
            var item = items.Where(i => i.IsActive).First();

            if (navOptions.IsAjax)
            {
                var urlHelper = htmlHelper.GetUrlHelper();
                var url = urlHelper.DefaultAction(item.Action, item.Controller, item.Area, item.RouteName, null, navOptions.RouteValues);
                return MvcHtmlStringCompatibility.Empty().AppendFormat("$('#{0}').load('{1}');", navOptions.AjaxTarget, url);
            }
            else
            {
                if (item.RouteValues is RouteValueDictionary)
                {
                    return htmlHelper.Action(item.Action, item.Controller, item.RouteValues as RouteValueDictionary).AsHtmlContent();
                }
                else
                {
                    return htmlHelper.Action(item.Action, item.Controller, item.RouteValues).AsHtmlContent();
                }
            }
        }
    }
#endif

    /// <summary>
    /// Represents options of the navigation menu.
    /// </summary>
    public class NavMenuOptions
    {
        /// <summary>
        /// Creates a new instance of NavMenuOptions with default options.
        /// </summary>
        public NavMenuOptions()
        {
            NavCssClasses = "";
            MatchAction = true;
            Icon = "";
            IconActive = "";
            AjaxTarget = "menu-target";
        }

        /// <summary>
        /// Gets or sets the CSS classes for navigation menu.
        /// </summary>
        public string NavCssClasses { get; set; }
        /// <summary>
        /// Gets or sets route values.
        /// </summary>
        public object RouteValues { get; set; }
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public Dictionary<string, bool> Status { get; set; }
        /// <summary>
        /// Indicates whether to match the action or not.
        /// </summary>
        public bool MatchAction { get; set; }
        /// <summary>
        /// Indicates whether to match action by status or not.
        /// </summary>
        public bool MatchActiveByStatus { get; set; }
        /// <summary>
        /// Gets or sets the icon of the navigation menu.
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// Gets or sets the active icon of the navigation menu.
        /// </summary>
        public string IconActive { get; set; }
        /// <summary>
        /// Indicates whether the navigation menu is making AJAX call or not. 
        /// </summary>
        public bool IsAjax { get; set; }
        /// <summary>
        /// Gets or sets the id of HTML "div" element which to be populated by AJAX call back. 
        /// </summary>
        public string AjaxTarget { get; set; }
    }

    /// <summary>
    /// Represents the navigation tab.
    /// </summary>
    public class NavTabItem
    {
        /// <summary>
        /// Creates a new instance of NavTabItem.
        /// </summary>
        public NavTabItem()
        {
            this.IconActive = "";
        }

        /// <summary>
        /// Creates a new instance of NavTabItem with specified MenuNode.
        /// </summary>
        /// <param name="menuNode">The menu node.</param>
        public NavTabItem(MenuNode menuNode) : this()
        {
            this.LinkText = menuNode.Title;
            this.Action = menuNode.Action;
            this.Controller = menuNode.Controller;
            this.Area = menuNode.Area;
            this.RouteName = menuNode.RouteName;
            this.Icon = menuNode.Icon;
        }

        /// <summary>
        /// Gets or sets the text of the link.
        /// </summary>
        public string LinkText { get; set; }     //Defaults to Action if not set
        /// <summary>
        /// Gets or sets the action name.
        /// </summary>
        public string Action { get; set; }       //Must be set
        /// <summary>
        /// Gets or sets the controller name.
        /// </summary>
        public string Controller { get; set; }   //Defaults to current controller if not set
        /// <summary>
        /// Gets or sets the area name.
        /// </summary>
        public string Area { get; set; }         //Defaults to currrent area if not set
        /// <summary>
        /// Gets or sets the route name.
        /// </summary>
        public string RouteName { get; set; }
        /// <summary>
        /// Gets or sets the icon of the link.
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// Gets or sets the active icon.
        /// </summary>
        public string IconActive { get; set; }   //If Icon is set, activeIcon defaults to "icon-ok" also used for when status is true
        /// <summary>
        /// Gets or sets the route values.
        /// </summary>
        public object RouteValues { get; set; }
        /// <summary>
        /// Indicates whether the link has status or not.
        /// </summary>
        public bool HasStatus { get; set; }
        /// <summary>
        /// Indicates whether the link is active or not.
        /// </summary>
        public bool IsActive { get; set; }       // For default when using Ajax menus

#if NET_4X
        /// <summary>
        /// Sets the navigation tab to active.
        /// </summary>
        /// <param name="items">The list of NavTabItem.</param>
        /// <param name="defaultIndex">The default index.</param>
        public static void SetActive(List<NavTabItem> items, int defaultIndex = 0)
        {
            if (items == null || items.Count == 0) return;
            var routeValues = MenuNode.GetActiveCommand();
            if (routeValues == null)
            {
                items[defaultIndex].IsActive = true;
                return;
            }
            var action = routeValues["action"].ToString();
            var item = items.Where(i => i.Action.Equals(action, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? items[defaultIndex];
            item.IsActive = true;
            routeValues.Remove("action");
            item.RouteValues = routeValues;
        }
#endif
    }
}