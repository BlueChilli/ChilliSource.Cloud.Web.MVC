using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
#endif

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
        PageContainerLeft,
        PageContainerTop,
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
            Actions = new List<IHtmlContent>();
            Buttons = new List<IHtmlContent>();
        }

        public string SaveMenuText { get; set; }

        public string CancelMenuUrl { get; set; }

        public bool? ShowButtons { get; set; }

        public List<IHtmlContent> Buttons { get; set; }

        public bool ShowActions { get; set; }

        public List<IHtmlContent> Actions { get; set; }

        public bool Dropup { get; set; }
    }

    public class Template_PageMessage
    {
        public const string Success = "PageMessage_Success";
        public const string Success_Static = "PageMessage_Success_Static";
        public const string Warning = "PageMessage_Warning";
        public const string Warning_Static = "PageMessage_Warning_Static";
        public const string Info = "PageMessage_Info";
        public const string Info_Static = "PageMessage_Info_Static";

        public static List<Template_PageMessage> MessageTypes = new List<Template_PageMessage>
        {
            new Template_PageMessage { Key = Success, MessageClass = "success" },
            new Template_PageMessage { Key = Success_Static, MessageClass = "success", IsStatic = true },
            new Template_PageMessage { Key = Warning, MessageClass = "warning" },
            new Template_PageMessage { Key = Warning_Static, MessageClass = "warning", IsStatic = true },
            new Template_PageMessage { Key = Info, MessageClass = "info" },
            new Template_PageMessage { Key = Info_Static, MessageClass = "info", IsStatic = true }
        };

        public string Key { get; set; }

        public string MessageClass { get; set; }

        public bool IsStatic { get; set; }
    }
}
