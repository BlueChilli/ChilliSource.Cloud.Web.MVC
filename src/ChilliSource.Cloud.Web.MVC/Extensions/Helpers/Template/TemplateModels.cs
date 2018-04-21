using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// To extend TemplateType create another enum eg MyTemplateType and create helpers for it
    /// </summary>
    public enum TemplateType
    {
        StandardField = 1,
        VerticalField,
        OptionalField,
        ModalField,
        HiddenField,
        PageContainer,
        PageButtons,
        PageMessage,
        ValidationSummary,
        Button
    }

    public class Template_PageButtons
    {
        public Template_PageButtons()
        {
            SaveMenuText = "Save";
            Actions = new List<MvcHtmlString>();
            Buttons = new List<MvcHtmlString>();
        }

        public string SaveMenuText { get; set; }

        public string CancelMenuUrl { get; set; }

        public bool? ShowButtons { get; set; }

        public List<MvcHtmlString> Buttons { get; set; }

        public bool ShowActions { get; set; }

        public List<MvcHtmlString> Actions { get; set; }

        public bool Dropup { get; set; }
    }

    public class Template_PageMessage
    {
        public const string Success = "PageMessage_Success";
        public const string Warning = "PageMessage_Warning";

        public static List<Template_PageMessage> MessageTypes = new List<Template_PageMessage> { new Template_PageMessage { Key = Success, MessageClass = "success" }, new Template_PageMessage { Key = Warning, MessageClass = "warning" } };

        public string Key { get; set; }

        public string MessageClass { get; set; }
    }
}
