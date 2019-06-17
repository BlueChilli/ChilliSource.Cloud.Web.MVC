using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using ChilliSource.Cloud.Core;

#if NET_4X
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
#endif


namespace ChilliSource.Cloud.Web.MVC
{

    public class TemplateOptions
    {
        public static TemplateType DefaultTemplateType { get; set; }

        public TemplateOptions()
        {
            Initialize();
        }

        TemplateType _template;
        public TemplateType Template { get { return _template; } set { _template = value ?? throw new ArgumentNullException(nameof(Template)); } }

        public string Label { get; set; }

        public string HelpText { get; set; }

        public int? FieldColumnSize { get; set; }

        public FieldTemplateSize? FieldSize { get; set; }

        public bool? IsMandatory { get; set; }

        protected void Initialize()
        {
            Template = TemplateOptions.DefaultTemplateType ??
                        throw new ApplicationException("DefaultTemplateType needs to be set via TemplateOptions.DefaultTemplateType");
        }
    }

    public class FieldTemplateModel
    {
        public const string InnerTemplateMarker = "###InnerTemplate###";

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

    public interface IFieldInnerTemplateModel
    {
        string Id { get; set; }

        string Name { get; set; }

        object Value { get; set; }

        string DisplayName { get; set; }

        FieldInnerTemplateMetadata InnerMetadata { get; set; }

        RouteValueDictionary HtmlAttributes { get; set; }

        IFieldInnerTemplateModel<TOptionsNew> UseOptions<TOptionsNew>(TOptionsNew options = null) where TOptionsNew : FieldTemplateOptionsBase;
        FieldTemplateOptionsBase GetOptions();
    }

    public class FieldInnerTemplateMetadata
    {
        public FieldInnerTemplateMetadata(ModelMetadata modelMetadata, object modelValue, MemberExpression memberExpression, Type memberType)
        {
            this.ModelMetadata = modelMetadata;
            this.ModelValue = modelValue;
            this.MemberExpression = memberExpression;

            this.MemberType = memberType;
            this.MemberUnderlyingType = modelMetadata.IsNullableValueType ? Nullable.GetUnderlyingType(memberType) : memberType;
        }

        public ModelMetadata ModelMetadata { get; }
        public object ModelValue { get; }
        public MemberExpression MemberExpression { get; }

        public Type MemberType { get; }
        public Type MemberUnderlyingType { get; }
    }

    public interface IFieldInnerTemplateModel<TOptions> : IFieldInnerTemplateModel
        where TOptions : class
    {
        TOptions Options { get; }
    }

    internal class FieldInnerTemplateModel<TOptions> : IFieldInnerTemplateModel<TOptions>
        where TOptions : FieldTemplateOptionsBase
    {
        FieldInnerTemplateModel _templateModel;

        public FieldInnerTemplateModel() : this(null, null) { }
        public FieldInnerTemplateModel(FieldInnerTemplateModel templateModel, TOptions options)
        {
            _templateModel = templateModel ?? new FieldInnerTemplateModel();

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            this.Options = options;
        }
        public TOptions Options { get; }
        public string Id { get => _templateModel.Id; set => _templateModel.Id = value; }
        public string Name { get => _templateModel.Name; set => _templateModel.Name = value; }
        public object Value { get => _templateModel.Value; set => _templateModel.Value = value; }
        public string DisplayName { get => _templateModel.DisplayName; set => _templateModel.DisplayName = value; }
        public RouteValueDictionary HtmlAttributes { get => _templateModel.HtmlAttributes; set => _templateModel.HtmlAttributes = value; }
        public FieldInnerTemplateMetadata InnerMetadata { get => _templateModel.InnerMetadata; set => _templateModel.InnerMetadata = value; }

        public IFieldInnerTemplateModel<TOptionsNew> UseOptions<TOptionsNew>(TOptionsNew options)
            where TOptionsNew : FieldTemplateOptionsBase
        {
            if (typeof(TOptions) == typeof(TOptionsNew) && Object.ReferenceEquals(options, this.Options))
                return (IFieldInnerTemplateModel<TOptionsNew>)this;

            return new FieldInnerTemplateModel<TOptionsNew>(_templateModel, options: options);
        }

        public FieldTemplateOptionsBase GetOptions()
        {
            return this.Options;
        }
    }

    public class FieldInnerTemplateModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }

        public FieldInnerTemplateMetadata InnerMetadata { get; set; }

        public string DisplayName { get; set; }

        public RouteValueDictionary HtmlAttributes { get; set; }

        public IFieldInnerTemplateModel<TOptionsNew> UseOptions<TOptionsNew>(TOptionsNew options = null)
            where TOptionsNew : FieldTemplateOptionsBase
        {
            return new FieldInnerTemplateModel<TOptionsNew>(this, options: options);
        }

        public IFieldInnerTemplateModel UseOptions(FieldTemplateOptionsBase fieldOptions)
        {
            if (fieldOptions == null)
                throw new ArgumentNullException(nameof(fieldOptions));

            var cast = this.UseOptions((dynamic)fieldOptions);
            return (IFieldInnerTemplateModel)cast;
        }
    }

    public abstract class FieldTemplateOptionsBase
    {
        protected FieldTemplateOptionsBase()
        {
            AutoWireUpJavascript = true;
        }

        protected FieldTemplateOptionsBase(FieldTemplateOptionsBase other)
        {
            this.HtmlAttributes = other.HtmlAttributes;
            this.AutoWireUpJavascript = other.AutoWireUpJavascript;
        }

        public object HtmlAttributes { get; set; }

        public bool AutoWireUpJavascript { get; set; }

#if NET_4X
        public virtual IFieldInnerTemplateModel CreateFieldInnerTemplateModel<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            object model = metadata.Model;
#else
        public virtual IFieldInnerTemplateModel CreateFieldInnerTemplateModel<TModel, TValue>(IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var explorer = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
            ModelMetadata metadata = explorer.Metadata;
            object model = explorer.Model;
#endif

            var member = expression.Body as MemberExpression;
            var name = html.NameFor(expression).ToString();

            string attemptedValue = null;
            if (html.ViewContext.ViewData.ModelState.ContainsKey(name))
            {
                var kvp = html.ViewContext.ViewData.ModelState[name];
#if NET_4X
                attemptedValue = kvp.Value?.AttemptedValue;
#else
                attemptedValue = kvp.AttemptedValue;
#endif
            }

            var attributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(this.HtmlAttributes);
            string dataId = null;
            if (attributes.ContainsKey("Id"))
            {
                dataId = attributes["Id"].ToString();
                attributes.Remove("Id");
            }
            dataId = dataId ?? html.IdFor(expression).ToString();

#if NET_4X
            var validationAttributes = new RouteValueDictionary(html.GetUnobtrusiveValidationAttributes(name, metadata));
#else
            var validator = html.ViewContext.HttpContext.RequestServices.GetService<ValidationHtmlAttributeProvider>();
            var validationAttributes = new Dictionary<string, string>();
            validator?.AddAndTrackValidationAttributes(html.ViewContext, explorer, name, validationAttributes);
#endif
            attributes.Merge(validationAttributes);

            var innerMetadata = new FieldInnerTemplateMetadata(metadata, model, member, typeof(TValue));
            var data = new FieldInnerTemplateModel
            {
                Id = dataId,
                Name = name,
                Value = String.IsNullOrEmpty(attemptedValue) || (model != null && attemptedValue == model.ToString()) ? model : attemptedValue,
                DisplayName = html.GetLabelTextFor(expression),
                HtmlAttributes = attributes,
                InnerMetadata = innerMetadata
            };

            var templateModel = data.UseOptions(this);

            return templateModel;
        }

        public virtual IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            return templateModel;
        }

        public abstract string GetViewPath();
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

    public class FieldTemplateOptions : FieldTemplateOptionsBase
    {
        public FieldTemplateOptions() { }

        public static IHtmlContent AddOn(string text)
        {
            return MvcHtmlStringCompatibility.Create($"<span class=\"input-group-addon\">{text}</span>");
        }

        public static IHtmlContent ButtonAddOn(string text, string classes = "")
        {
            return MvcHtmlStringCompatibility.Create($"<span class=\"input-group-btn\"><button type=\"button\" class=\"btn {classes}\">{text}</button></span>");
        }

#if NET_4X
        public override IFieldInnerTemplateModel CreateFieldInnerTemplateModel<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
#else
        public override IFieldInnerTemplateModel CreateFieldInnerTemplateModel<TModel, TValue>(IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
#endif
            var templateModel = base.CreateFieldInnerTemplateModel(html, expression);

            var valueTypeName = templateModel.InnerMetadata.MemberUnderlyingType.Name;
            var valueBaseType = templateModel.InnerMetadata.MemberUnderlyingType.BaseType;
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var member = templateModel.InnerMetadata.MemberExpression;

            if (metadata.IsReadOnly)
            {
                templateModel = templateModel.UseOptions(new ReadonlyFieldTemplateOptions(this));
            }
            else if (metadata.AdditionalValues.ContainsKey("Checkbox"))
            {
                templateModel = templateModel.UseOptions(new CheckboxFieldTemplateOptions(this));
            }
            else if (metadata.AdditionalValues.ContainsKey("Radio"))
            {
                if (valueBaseType == typeof(Enum))
                {
                    templateModel = templateModel.UseOptions(new RadioListFieldTemplateOptions(this));
                }
                else
                {
                    templateModel = templateModel.UseOptions(new RadioFieldTemplateOptions(this));
                }
            }
            else if (valueBaseType == typeof(Enum))
            {
                templateModel = templateModel.UseOptions(new SelectFieldTemplateOptions(this));
            }
            else if (valueTypeName == "DateTime")
            {
                templateModel = templateModel.UseOptions(new DateFieldTemplateOptions(this));
            }
            else if (valueTypeName == "Int32")
            {
                var minMax = IntegerDropDownAttribute.ResolveAttribute(metadata, member);
                if (minMax != null)
                {
                    var selectedValue = templateModel.Value.ToNullable<int>();
                    int min = minMax.Min;
                    int max = minMax.Max;
                    var intList = Enumerable.Range(min, max - min + 1).Select(i => new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = selectedValue.HasValue && selectedValue.Value == i }).ToList();
                    if (minMax.IsReverse)
                        intList.Reverse();

                    var selectOptions = new SelectFieldTemplateOptions(this)
                    {
                        SelectList = intList
                    };

                    templateModel = templateModel.UseOptions(selectOptions);
                }
                else
                {
                    templateModel = templateModel.UseOptions(new InputFieldTemplateOptions(this));
                }
            }
            else if (valueTypeName == "HttpPostedFileBase" || valueTypeName == "IFormFile")
            {
                templateModel = templateModel.UseOptions(new FileFieldTemplateOptions(this));
            }
            else
            {
                templateModel = templateModel.UseOptions(new InputFieldTemplateOptions(this));
            }

            return templateModel;
        }

        public override string GetViewPath()
        {
            throw new NotSupportedException("GetViewPath method is not supported in the default FieldTemplateOptions.");
        }
    }

    public class InputFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/Input";

        public InputFieldTemplateOptions() : base() { }
        public InputFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public IHtmlContent PreAddOn { get; set; }

        public IHtmlContent PostAddOn { get; set; }

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }

        public static void ResolveInputAttributes(IFieldInnerTemplateModel templateModel, string inputType)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var member = templateModel.InnerMetadata.MemberExpression;

            PlaceholderAttribute.Resolve(metadata, templateModel.HtmlAttributes);
            HtmlHelperExtensions.ResolveStringLength(member, templateModel.HtmlAttributes);
            templateModel.HtmlAttributes.AddOrSkipIfExists("type", inputType);
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var member = templateModel.InnerMetadata.MemberExpression;
            var modelValue = templateModel.InnerMetadata.ModelValue;

            var inputType = "text";

            switch (templateModel.InnerMetadata.MemberUnderlyingType.Name)
            {
                case "Boolean":
                    {
                        inputType = metadata.AdditionalValues.ContainsKey("Radio") ? "radio" : "checkbox";
                        if (HtmlHelperExtensions.ConvertAttemptedValueToBoolean(templateModel.Value))
                        {
                            templateModel.HtmlAttributes.AddOrSkipIfExists("checked", "checked");
                        }
                        templateModel.Value = Boolean.TrueString;
                        break;
                    }
                case "Int32":
                    {
                        if (metadata.DataTypeName == null || metadata.DataTypeName == DataType.Currency.ToString())
                        {
                            inputType = "number";
                        }
                        break;
                    }
                case "Decimal":
                    {
                        if (metadata.DataTypeName == null || metadata.DataTypeName == DataType.Currency.ToString())
                        {
                            inputType = "number";
                            templateModel.HtmlAttributes.Add("Step", "any");
                        }
                        break;
                    }
                default:
                    {
                        switch (metadata.DataTypeName)
                        {
                            case "Password":
                                {
                                    inputType = "password";
                                    templateModel.Value = "";
                                    break;
                                }
                            case "Html":
                                {
                                    return templateModel.UseOptions(new HtmlFieldTemplateOptions(this));
                                }
                            case "MultilineText":
                                {
                                    return templateModel.UseOptions(new TextAreaFieldTemplateOptions(this));
                                }
                            case "EmailAddress":
                                {
                                    inputType = "Email";
                                    break;
                                }
                            case "Url":
                                {
                                    inputType = "Url";
                                    break;
                                }
                        }
                        break;
                    }
            }

            switch (metadata.DataTypeName)
            {
                case "Currency":
                    this.PreAddOn = MvcHtmlStringCompatibility.Create("<span class=\"input-group-addon\">$</span>");
                    break;

            }

            ResolveInputAttributes(templateModel, inputType);

            return templateModel;
        }
    }

    public abstract class SelectListFieldTemplateOptionsBase : FieldTemplateOptionsBase
    {
        private static readonly SelectListItem[] SingleEmptyItem = { new SelectListItem { Text = "", Value = "" } };

        public SelectListFieldTemplateOptionsBase() : base() { }
        public SelectListFieldTemplateOptionsBase(FieldTemplateOptionsBase other) : base(other) { }

        public IEnumerable<SelectListItem> SelectList { get; set; }

        protected void ProcessSelect(Type baseType, ModelMetadata metadata, IFieldInnerTemplateModel data)
        {
            if (this.SelectList != null)
            {
                if (metadata.ModelType.IsGenericType && metadata.ModelType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    data.HtmlAttributes.Add("multiple", "multiple");
                    var selectedValues = new HashSet<string>((data.Value as IEnumerable)?.Cast<object>().Select(v => v?.ToString())
                                                                ?? Enumerable.Empty<string>());

                    foreach (var item in this.SelectList)
                    {
                        if (selectedValues.Contains(item.Value))
                            item.Selected = true;
                    }
                }
                else
                {
                    var selectedValue = data.Value?.ToString();
                    foreach (var item in this.SelectList)
                    {
                        if (item.Value == selectedValue)
                            item.Selected = true;
                    }
                }
            }
            else if (baseType == typeof(Enum))
            {
                Type enumType = Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType;
                var values = EnumExtensions.GetValues(enumType).Cast<object>();
                var modelValues = data.Value == null ? new string[0] : data.Value.ToString().Split(',');
                for (var i = 0; i < modelValues.Count(); i++) modelValues[i] = modelValues[i].Trim();

                this.SelectList =
                    from v in values
                    select new SelectListItem
                    {
                        Text = EnumExtensions.GetEnumDescription(v),
                        Value = v.ToString(),
                        Selected = modelValues.Contains(v.ToString())
                    };
                var flags = enumType.GetCustomAttribute<FlagsAttribute>();
                if (flags != null && !data.HtmlAttributes.ContainsKey("multiple"))
                    data.HtmlAttributes.Add("multiple", "multiple");

                this.SelectList = EmptyItemAttribute.Resolve(metadata, this.SelectList, SingleEmptyItem);
                this.SelectList = RemoveItemAttribute.Resolve(metadata, this.SelectList);
            }
        }
    }

    public class SelectFieldTemplateOptions : SelectListFieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/Select";

        public SelectFieldTemplateOptions() : base() { }
        public SelectFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var baseType = templateModel.InnerMetadata.MemberUnderlyingType.BaseType;
            base.ProcessSelect(baseType, metadata, templateModel);

            return templateModel;
        }
    }

    public class HtmlFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/Html";

        public HtmlFieldTemplateOptions() : base() { }
        public HtmlFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            InputFieldTemplateOptions.ResolveInputAttributes(templateModel, "hidden");

            return templateModel;
        }
    }

    public class TextAreaFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/TextArea";

        public TextAreaFieldTemplateOptions() : base() { }
        public TextAreaFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            InputFieldTemplateOptions.ResolveInputAttributes(templateModel, "text");

            return templateModel;
        }
    }

    public class DateFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/Date";

        public DateFieldTemplateOptions() : base() { }
        public DateFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }

        public DateFormatAttribute DateFormatAttribute { get; set; }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var member = templateModel.InnerMetadata.MemberExpression;

            if (templateModel.Value is String)
            {
                var dateParts = ((string)templateModel.Value).Split(',');
                if (dateParts.Length >= 2 && !String.IsNullOrEmpty(dateParts[0]) && !String.IsNullOrEmpty(dateParts[1]))
                {
                    if (dateParts.Length == 2)
                    {
                        templateModel.Value = new DateTime(int.Parse(dateParts[1]), int.Parse(dateParts[0]), 1);
                    }
                    else
                    {
                        templateModel.Value = new DateTime(int.Parse(dateParts[2]), int.Parse(dateParts[1]), int.Parse(dateParts[0]));
                    }
                }
                else
                {
                    DateTime d;
                    if (!String.IsNullOrEmpty(metadata.DisplayFormatString) && DateTime.TryParseExact((string)templateModel.Value, metadata.DisplayFormatString, null, System.Globalization.DateTimeStyles.None, out d))
                    {
                        templateModel.Value = d;
                    }
                    else if (DateTime.TryParse((string)templateModel.Value, out d))
                    {
                        templateModel.Value = d;
                    }
                    else
                    {
                        templateModel.Value = null;
                    }
                }
            }

            if (this.DateFormatAttribute == null)
            {
                var dateFormatAttribute = member.Member.GetCustomAttribute<DateFormatAttribute>();
                if (dateFormatAttribute == null)
                {
                    return templateModel.UseOptions(new DatePickerFieldTemplateOptions(this));
                }
                else
                {
                    this.DateFormatAttribute = dateFormatAttribute;
                }
            }

            return templateModel;
        }
    }

    public class DatePickerFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/DatePicker";

        public DatePickerFieldTemplateOptions() : base() { }
        public DatePickerFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }

        public IHtmlContent PreAddOn { get; set; }

        public IHtmlContent PostAddOn { get; set; }
    }

    public class ClockPickerFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/ClockPicker";

        public ClockPickerFieldTemplateOptions() : base() { }
        public ClockPickerFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }
    }

    public class DateTimePickerFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/DateTimePicker";

        public DateTimePickerFieldTemplateOptions() : base() { }
        public DateTimePickerFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }

        public IHtmlContent PreAddOn { get; set; }

        public IHtmlContent PostAddOn { get; set; }
    }

    public class CheckboxFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/Checkbox";

        public CheckboxFieldTemplateOptions() : base() { }
        public CheckboxFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }

        public CheckBoxAttribute CheckBoxAttribute { get; set; }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var member = templateModel.InnerMetadata.MemberExpression;

            if (HtmlHelperExtensions.ConvertAttemptedValueToBoolean(templateModel.Value))
            {
                templateModel.HtmlAttributes.AddOrSkipIfExists("checked", "checked");
            }
            templateModel.HtmlAttributes.AddOrSkipIfExists("type", "checkbox");

            if (this.CheckBoxAttribute == null)
                this.CheckBoxAttribute = member.Member.GetCustomAttribute<CheckBoxAttribute>();

            return templateModel;
        }
    }

    public class ReadonlyFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/ReadOnly";

        public ReadonlyFieldTemplateOptions() : base() { }
        public ReadonlyFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var modelValue = templateModel.InnerMetadata.ModelValue;


            if (!String.IsNullOrEmpty(metadata.DisplayFormatString))
            {
                templateModel.Value = String.Format("{" + metadata.DisplayFormatString + "}", modelValue);
            }

            //TODO: select list support ?
            //else if (data.Options.SelectList != null && data.Options.SelectList.Any(x => x.Value == data.Value?.ToString()))
            //{
            //    data.Value = data.Options.SelectList.First(x => x.Value == data.Value.ToString()).Text;
            //}
            //else if (data.Options.SelectList != null && data.Options.SelectList.Any(x => x.Value == data.Value.ToString()))
            //{
            //    data.Value = data.Options.SelectList.First(x => x.Value == data.Value.ToString()).Text;
            //}

            return templateModel;
        }
    }

    public class RadioFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/Radio";

        public RadioFieldTemplateOptions() : base() { }
        public RadioFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }

        public RadioAttribute RadioAttribute { get; set; }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var member = templateModel.InnerMetadata.MemberExpression;

            if (this.RadioAttribute == null)
                this.RadioAttribute = member.Member.GetCustomAttribute<RadioAttribute>() ?? new RadioAttribute();

            return templateModel;
        }
    }

    public class RadioListFieldTemplateOptions : SelectListFieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/RadioList";

        public RadioListFieldTemplateOptions() : base() { }
        public RadioListFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }

        public RadioAttribute RadioAttribute { get; set; }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            var member = templateModel.InnerMetadata.MemberExpression;
            var baseType = templateModel.InnerMetadata.MemberUnderlyingType.BaseType;

            if (this.RadioAttribute == null)
                this.RadioAttribute = member.Member.GetCustomAttribute<RadioAttribute>() ?? new RadioAttribute();

            base.ProcessSelect(baseType, metadata, templateModel);

            return templateModel;
        }
    }

    public class FileFieldTemplateOptions : FieldTemplateOptionsBase
    {
        public static string PartialViewLocation { get; set; } = "FieldTemplates/File";

        public FileFieldTemplateOptions() : base() { }
        public FileFieldTemplateOptions(FieldTemplateOptionsBase other) : base(other) { }

        public string ButtonText { get; set; } = "Choose";

        public override string GetViewPath()
        {
            return PartialViewLocation;
        }

        public override IFieldInnerTemplateModel ProcessInnerField(IFieldInnerTemplateModel templateModel)
        {
            var metadata = templateModel.InnerMetadata.ModelMetadata;
            HttpPostedFileExtensionsAttribute.Resolve(metadata, templateModel.HtmlAttributes);

            return templateModel;
        }
    }
}
