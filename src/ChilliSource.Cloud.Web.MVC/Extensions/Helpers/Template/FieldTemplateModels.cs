using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NET_4X
using System.Web.Mvc;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
 
    public class TemplateOptions
    {
        public TemplateOptions()
        {
            Initialize();
        }

        public TemplateType Template { get; set; }

        public string Label { get; set; }

        public string HelpText { get; set; }

        public int? FieldColumnSize { get; set; }

        public FieldTemplateSize? FieldSize { get; set; }

        public bool? IsMandatory { get; set; }

        protected void Initialize()
        {
            Template = TemplateType.StandardField;
        }
    }

    public class FieldTemplateOptions
    {
        public FieldTemplateOptions()
        {
            AutoWireUpJavascript = true;
        }

        public object HtmlAttributes { get; set; }

        public IEnumerable<SelectListItem> SelectList { get; set; }

        public InnerTemplateType TemplateType { get; set; }

        public bool AutoWireUpJavascript { get; set; }

        public object CustomOptions { get; set; }

        public IHtmlContent PreAddOn { get; set; }

        public IHtmlContent PostAddOn { get; set; }

        public static IHtmlContent AddOn(string text)
        {
            return MvcHtmlStringCompatibility.Create($"<span class=\"input-group-addon\">{text}</span>");
        }

        public static IHtmlContent ButtonAddOn(string text, string classes = "")
        {
            return MvcHtmlStringCompatibility.Create($"<span class=\"input-group-btn\"><button type=\"button\" class=\"btn {classes}\">{text}</button></span>");
        }
    }

    public class FieldTemplateModel
    {
        public string ModelName { get; set; }

        public string DisplayName { get; set; }

        public bool IsMandatory { get; set; }

        public string HelpText { get; set; }

        public int? FieldColumnSize { get; set; }

        public FieldTemplateSize? FieldSize { get; set; }

        public string GetFieldSize(FieldTemplateSize defaultSize = FieldTemplateSize.Medium)
        {
            switch (FieldSize.GetValueOrDefault(defaultSize))
            {
                case FieldTemplateSize.ExtraSmall: return "col-lg-2 col-md-3 col-sm-4 col-xs-6";
                case FieldTemplateSize.Small: return "col-lg-3 col-md-4 col-sm-6";
                case FieldTemplateSize.Medium: return "col-xl-4 col-lg-6 col-md-6";
                case FieldTemplateSize.Large: return "col-lg-9";
                case FieldTemplateSize.ExtraLarge: return "col-lg-12";
            }
            return "";
        }

    }

    public enum FieldTemplateSize
    {
        ExtraSmall,
        Small,
        Medium,
        Large,
        ExtraLarge  //Full-width
    }

    public enum InnerTemplateType
    {
        Default = 0,
        Input,
        Select,
        Html,
        TextArea,
        Date,
        DatePicker,
        ClockPicker,
        DateTimePicker,
        Checkbox,
        ReadOnly,
        Radio,
        RadioList,
        File
    }

    public class FieldInnerTemplateModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }

        public string DisplayName { get; set; }

        public FieldTemplateOptions Options { get; set; }

        public RouteValueDictionary HtmlAttributes { get; set; }
    }



    public class Html_CustomOptions
    {
        public Html_CustomOptions()
        {
            Toolbar = new List<KeyValuePair<string, List<string>>>
            {
                new KeyValuePair<string, List<string>>("group1", new List<string> { "bold", "link" })
            };
        }

        public List<KeyValuePair<string, List<string>>> Toolbar { get; set; }

        public bool IsInModal { get; set; }

        public IHtmlContent ToolbarJson()
        {
            var items = Toolbar.Select(g => $"['{g.Key}', [{g.Value.Select(i => $"'{i}'").ToDelimitedString(",")}]]").ToList();
            return MvcHtmlStringCompatibility.Create($"[{items.ToDelimitedString(",")}]");
        }

    }

}
