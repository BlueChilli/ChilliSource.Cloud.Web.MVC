using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
#if NET_4X
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    //If root is an area it's area value MUST be set. Roots must be defined before any children
    //First node is RootNode
    /// <summary>
    /// Represents the base functionality for all menus.
    /// </summary>
    public abstract class MenuBase
    {
        /// <summary>
        /// Root node.
        /// </summary>
        public static MenuNode RootNode = new MenuNode();
        /// <summary>
        /// Menu type.
        /// </summary>
        public static Type MenuType;

        #region Build
        //Call from Application_Start()
        /// <summary>
        /// Construct menus by menu type.
        /// </summary>
        /// <param name="menuType">The type.</param>
        public static void Build(Type menuType)
        {
            MenuType = menuType;

            var fields = menuType.GetFields();
            if (fields.Count() > 0) MenuBase.RootNode = fields[0].GetValue(null) as MenuNode;
            foreach (var field in fields)
            {
                BuildChildren(field, fields);
            }
        }

        /// <summary>
        /// Construct Relationship between parent and children nodes.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="children">The list of children nodes.</param>
        public static void SetCustomRelationship(MenuNode parent, List<MenuNode> children)
        {
            parent.Children = children;
            children.ForEach(c => c.Parent = parent);
        }

        /// <summary>
        /// Constructs children menu nodes.
        /// </summary>
        /// <param name="parentField">The parent node.</param>
        /// <param name="fields">The fields attributes.</param>
        private static void BuildChildren(FieldInfo parentField, FieldInfo[] fields)
        {
            var parentValue = parentField.GetValue(null) as MenuNode;
            parentValue.Id = parentValue.Id.DefaultTo(parentField.Name);

            //Set root value default. If root is an area it's value MUST be set. Roots must be defined before any children
            if (String.IsNullOrEmpty(parentValue.Controller))
            {
                parentValue.Controller = GetControllerFromName(parentField.Name, parentValue.Area);
            }

            foreach (var field in fields)
            {
                if (field.Name.StartsWith(parentField.Name) && field.Name != parentField.Name)
                {
                    if (field.Name.LastIndexOf('_') == parentField.Name.Length)
                    {
                        var fieldValue = field.GetValue(null) as MenuNode;

                        fieldValue.Id = fieldValue.Id.DefaultTo(field.Name);
                        fieldValue.Area = fieldValue.Area.DefaultTo(parentValue.Area);
                        fieldValue.Controller = fieldValue.Controller.DefaultTo(parentValue.Controller, GetControllerFromName(field.Name, parentValue.Area));
                        if (String.IsNullOrEmpty(parentValue.Controller) || parentValue.Controller == GetControllerFromName(field.Name, parentValue.Area))
                        {
                            fieldValue.Action = fieldValue.Action.DefaultTo(GetActionFromName(field.Name, parentValue.Area));
                        }
                        else
                        {   //Controller has been manually changed - reset depth of action name
                            //Find parent that was changed
                            var changedParent = parentValue;
                            while (changedParent.Parent != null && changedParent.Parent.Controller == fieldValue.Controller) changedParent = changedParent.Parent;
                            fieldValue.Action = GetActionFromName(field.Name.Substring(changedParent.Id.Length), "");
                        }
                        fieldValue.Parent = parentValue;

                        parentValue.Children.Add(fieldValue);
                    }
                }
            }
        }

        private static string GetControllerFromName(string menuName, string parentArea)
        {
            var nameParts = menuName.Split('_');
            if (String.IsNullOrEmpty(parentArea))
            {
                return nameParts[0];
            }
            else
            {
                return (nameParts.Length > 1) ? nameParts[1] : "";
            }

        }

        private static string GetActionFromName(string menuName, string parentArea)
        {
            var nameParts = menuName.Split('_');
            if (String.IsNullOrEmpty(parentArea))
            {
                if (nameParts.Length > 2) return String.Join("", nameParts, 1, nameParts.Length - 1);
                return (nameParts.Length > 1) ? nameParts[1] : "";
            }
            else
            {
                if (nameParts.Length > 3) return String.Join("", nameParts, 2, nameParts.Length - 2);
                return (nameParts.Length > 2) ? nameParts[2] : "";
            }

        }
        #endregion        
    }

    /// <summary>
    /// Represents the base functionality for menu html helper.
    /// </summary>
    public static class MenuHtmlHelper
    {
        /// <summary>
        /// Writes an opening <form> tag to the response and includes the route values in the action attribute. 
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="menuNode">The menu node to get the form Id, action and controller.</param>
        /// <param name="htmlAttributes">The specified html attribute object.</param>
        /// <returns>An instance of MvcForm object.</returns>
#if NET_4X
        public static MvcForm BeginForm<TModel>(this HtmlHelper<TModel> htmlHelper, MenuNode menuNode, object htmlAttributes = null)
#else
        public static MvcForm BeginForm<TModel>(this IHtmlHelper<TModel> htmlHelper, MenuNode menuNode, object htmlAttributes = null)
#endif        
        {
            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(htmlAttributes);
            if (!attributes.ContainsKey("id")) attributes["id"] = menuNode.GetIdAs(MenuIdType.Form).ToCssClass();

            // Set this attribute to String.Empty if you don't want form double submit prevention
            // This value is checked in jquery.bluechilli-mvc.js
            if (!attributes.ContainsKey("data-submitted")) attributes["data-submitted"] = "false";

            var method = FormMethod.Post;
            if (attributes.ContainsKey("method")) method = EnumExtensions.Parse<FormMethod>(attributes["method"].ToString());

            return htmlHelper.BeginForm(menuNode.Action, menuNode.Controller, method, attributes);
        }

#if NET_4X
        /// <summary>
        /// Invokes the specified child action method with the specified parameters and returns the result as an HTML string.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="menuNode">The menu node to get the form Id, action and controller from.</param>
        /// <param name="routeValues">An object that contains the parameters for a route used to generate the action.</param>
        /// <returns>HTML string for the action.</returns>
        public static IHtmlContent Action<TModel>(this HtmlHelper<TModel> htmlHelper, MenuNode menuNode, object routeValues = null)
        {
            return htmlHelper.Action(menuNode.Action, menuNode.Controller, routeValues).AsHtmlContent();
        }

        /// <summary>
        /// Returns HTML string for the Menu element.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="menuNode">The menu node to get the ViewData.</param>
        /// <param name="templateName">The template.</param>
        /// <returns>HTML string for the Menu element.</returns>
        public static IHtmlContent Menu(this HtmlHelper htmlHelper, MenuNode menuNode, string templateName)
        {
            return CreateHtmlHelperForModel(htmlHelper, menuNode)
                    .DisplayFor(m => menuNode, templateName).AsHtmlContent();
        }

        /// <summary>
        /// Initializes a new instance of the HtmlHelper class.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="model">The model.</param>
        /// <returns>An instance of the System.Web.Mvc.HtmlHelper.<TModel></returns>
        public static HtmlHelper<TModel> CreateHtmlHelperForModel<TModel>(HtmlHelper htmlHelper, TModel model)
        {
            return new HtmlHelper<TModel>(htmlHelper.ViewContext, new ViewDataContainer<TModel>(model));
        }
#endif
    }

#if NET_4X
    /// <summary>
    /// Encapsulates the model's view data dictionary.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    internal class ViewDataContainer<TModel> : IViewDataContainer
    {
        /// <summary>
        /// Initializes a new instance of the ViewDataContainer class by using the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public ViewDataContainer(TModel model)
        {
            ViewData = new ViewDataDictionary<TModel>(model);
        }

        /// <summary>
        /// Initializes a new instance of the ViewDataDictionary class by using the specified model.
        /// </summary>
        public ViewDataDictionary ViewData { get; set; }
    }
#endif

    /// <summary>
    /// An enumeration of menu type.
    /// </summary>
    public enum MenuIdType
    {
        /// <summary>
        /// The specified menu is a modal window.
        /// </summary>
        Modal,
        /// <summary>
        /// The specified menu is an HTML form.
        /// </summary>
        Form
    }

    #region Default option classes
    /// <summary>
    /// Encapsulates link html attributes.
    /// </summary>
    public class HtmlLinkFieldOptions : HtmlDefaultFieldOptions
    {
        /// <summary>
        /// Initialises a new instance of HtmlLinkFieldOptions class.
        /// </summary>
        public HtmlLinkFieldOptions()
        {
            this.Title = string.Empty;
            this.IconClasses = string.Empty;
        }

        /// <summary>
        /// Gets or sets the title of the link.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the Id of the link.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets route values of the link.
        /// </summary>
        public object RouteValues { get; set; }
        /// <summary>
        /// Gets or sets CSS icon class names of the link.
        /// </summary>
        public string IconClasses { get; set; }
        /// <summary>
        /// Gets or sets CSS link class names of the link.
        /// </summary>
        public string LinkClasses { get; set; }
        /// <summary>
        /// Gets or sets the host name of the link.
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// Gets or sets the fragment of the link.
        /// </summary>
        public string Fragment { get; set; }
    }

    /// <summary>
    /// Encapsulates list item attributes.
    /// </summary>
    public class HtmlListItemFieldOptions : HtmlDefaultFieldOptions
    {
    }

    /// <summary>
    /// Encapsulates html attributes.
    /// </summary>
    public class HtmlDefaultFieldOptions
    {
        /// <summary>
        /// Gets or sets HTML attributes of the field.
        /// </summary>
        public object HtmlAttributes { get; set; }
    }

    /// <summary>
    /// Class for passing url parameters to various methods
    /// </summary>
    public class MenuUrlValues
    {
        public MenuUrlValues()
        {

        }

        public MenuUrlValues(long id)
        {
            Id = id.ToString();
        }

        public string Id { get; set; }
        public object RouteValues { get; set; }
        public string Protocol { get; set; }
        public string Fragment { get; set; }
        public string HostName { get; set; }
    }
    #endregion
}
