using ChilliSource.Cloud.Core;
using ChilliSource.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#if NET_4X
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
#else
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class TemplateHelper
    {
        private static readonly SelectListItem[] SingleEmptyItem = { new SelectListItem { Text = "", Value = "" } };

#if NET_4X
        public static IHtmlContent FieldTemplateInnerFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return FieldTemplateInnerFor(html, expression, new FieldTemplateOptions { HtmlAttributes = htmlAttributes });
        }
#else        
        public static Task<IHtmlContent> FieldTemplateInnerForAsync<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return FieldTemplateInnerForAsync(html, expression, new FieldTemplateOptions { HtmlAttributes = htmlAttributes });
        }
#endif

#if NET_4X
        public static IHtmlContent FieldTemplateInnerFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, FieldTemplateOptions fieldOptions = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            object model = metadata.Model;
#else
        public static Task<IHtmlContent> FieldTemplateInnerForAsync<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, FieldTemplateOptions fieldOptions = null)
        {
            var explorer = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
            ModelMetadata metadata = explorer.Metadata;
            object model = explorer.Model;
#endif
            var member = expression.Body as MemberExpression;

            var httpContext = html.ViewContext.HttpContext;
            var request = httpContext.Request;
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

            if (fieldOptions == null) fieldOptions = new FieldTemplateOptions();
            var data = new FieldInnerTemplateModel
            {
                Id = html.IdFor(expression).ToString(),
                Name = name,
                Value = String.IsNullOrEmpty(attemptedValue) || (model != null && attemptedValue == model.ToString()) ? model : attemptedValue,
                DisplayName = html.GetLabelTextFor(expression),
                Options = fieldOptions,
                HtmlAttributes = RouteValueDictionaryHelper.CreateFromHtmlAttributes(fieldOptions.HtmlAttributes)
            };

            if (data.HtmlAttributes.ContainsKey("Id"))
            {
                data.Id = data.HtmlAttributes["Id"].ToString();
                data.HtmlAttributes.Remove("Id");
            }

#if NET_4X
            var validationAttributes = new RouteValueDictionary(html.GetUnobtrusiveValidationAttributes(data.Name, metadata));
#else
            var validator = html.ViewContext.HttpContext.RequestServices.GetService<ValidationHtmlAttributeProvider>();
            var validationAttributes = new Dictionary<string, string>();
            validator?.AddAndTrackValidationAttributes(html.ViewContext, explorer, data.Name, validationAttributes);
#endif
            data.HtmlAttributes.Merge(validationAttributes);

            string typeName = typeof(TValue).Name;
            Type baseType = typeof(TValue).BaseType;
            if (metadata.IsNullableValueType)
            {
                typeName = typeof(TValue).GetProperty("Value").PropertyType.Name;
                baseType = typeof(TValue).GetProperty("Value").PropertyType.BaseType;
            }

            if (data.Options.TemplateType == InnerTemplateType.Default)
            {
                if (metadata.IsReadOnly)
                {
                    data.Options.TemplateType = InnerTemplateType.ReadOnly;
                }
                else if (metadata.AdditionalValues.ContainsKey("Checkbox"))
                {
                    data.Options.TemplateType = InnerTemplateType.Checkbox;
                }
                else if (metadata.AdditionalValues.ContainsKey("Radio"))
                {
                    data.Options.TemplateType = baseType == typeof(Enum) || data.Options.SelectList != null ? InnerTemplateType.RadioList : InnerTemplateType.Radio;
                }
                else if (data.Options.SelectList != null)
                {
                    data.Options.TemplateType = InnerTemplateType.Select;
                }
                else if (baseType == typeof(Enum))
                {
                    data.Options.TemplateType = InnerTemplateType.Select;
                }
                else if (typeName == "DateTime")
                {
                    data.Options.TemplateType = InnerTemplateType.Date;
                }
                else if (typeName == "Int32")
                {
                    data.Options.TemplateType = InnerTemplateType.Input;
                    var minMax = IntegerDropDownAttribute.ResolveAttribute(metadata, member);
                    if (minMax != null)
                    {
                        data.Options.TemplateType = InnerTemplateType.Select;
                        var selectedValue = data.Value.ToNullable<int>();
                        int min = minMax.Min;
                        int max = minMax.Max;
                        var intList = Enumerable.Range(min, max - min + 1).Select(i => new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = selectedValue.HasValue && selectedValue.Value == i }).ToList();
                        if (minMax.IsReverse) intList.Reverse();
                        data.Options.SelectList = intList;
                    }
                }
                else if (typeName == "HttpPostedFileBase")
                {
                    data.Options.TemplateType = InnerTemplateType.File;
                }
                else
                {
                    data.Options.TemplateType = InnerTemplateType.Input;
                }
            }

            switch (data.Options.TemplateType)
            {
                case InnerTemplateType.Input:
                    {
                        var inputType = "text";

                        switch (typeName)
                        {
                            case "Boolean":
                                {
                                    inputType = metadata.AdditionalValues.ContainsKey("Radio") ? "radio" : "checkbox";
                                    if (HtmlHelperExtensions.ConvertAttemptedValueToBoolean(data.Value))
                                    {
                                        data.HtmlAttributes.AddOrSkipIfExists("checked", "checked");
                                    }
                                    data.Value = Boolean.TrueString;
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
                                        data.HtmlAttributes.Add("Step", "any");
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
                                                data.Value = "";
                                                break;
                                            }
                                        case "Html":
                                            {
                                                inputType = "hidden";
                                                data.Options.TemplateType = InnerTemplateType.Html;
                                                break;
                                            }
                                        case "MultilineText":
                                            {
                                                data.Options.TemplateType = InnerTemplateType.TextArea;
                                                break;
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
                                data.Options.PreAddOn = MvcHtmlStringCompatibility.Create("<span class=\"input-group-addon\">$</span>");
                                break;

                        }

                        PlaceholderAttribute.Resolve(metadata, data.HtmlAttributes);
                        HtmlHelperExtensions.ResolveStringLength(member, data.HtmlAttributes);
                        data.HtmlAttributes.AddOrSkipIfExists("type", inputType);
                        break;
                    }
                case InnerTemplateType.Select:
                    {
                        ProcessSelect<TValue>(baseType, metadata, data);
                        break;
                    }
                case InnerTemplateType.Date:
                    {
                        if (data.Value is String)
                        {
                            var dateParts = ((string)data.Value).Split(',');
                            if (dateParts.Length >= 2 && !String.IsNullOrEmpty(dateParts[0]) && !String.IsNullOrEmpty(dateParts[1]))
                            {
                                if (dateParts.Length == 2)
                                {
                                    data.Value = new DateTime(int.Parse(dateParts[1]), int.Parse(dateParts[0]), 1);
                                }
                                else
                                {
                                    data.Value = new DateTime(int.Parse(dateParts[2]), int.Parse(dateParts[1]), int.Parse(dateParts[0]));
                                }
                            }
                            else
                            {
                                DateTime d;
                                if (!String.IsNullOrEmpty(metadata.DisplayFormatString) && DateTime.TryParseExact((string)data.Value, metadata.DisplayFormatString, null, System.Globalization.DateTimeStyles.None, out d))
                                {
                                    data.Value = d;
                                }
                                else if (DateTime.TryParse((string)data.Value, out d))
                                {
                                    data.Value = d;
                                }
                                else
                                {
                                    data.Value = null;
                                }
                            }
                        }
                        var dateFormatAttribute = member.Member.GetCustomAttribute<DateFormatAttribute>();
                        if (dateFormatAttribute == null)
                        {
                            data.Options.TemplateType = InnerTemplateType.DatePicker;
                        }
                        else
                        {
                            data.Options.CustomOptions = dateFormatAttribute;
                        }
                        break;
                    }
                case InnerTemplateType.Checkbox:
                    {
                        if (HtmlHelperExtensions.ConvertAttemptedValueToBoolean(data.Value))
                        {
                            data.HtmlAttributes.AddOrSkipIfExists("checked", "checked");
                        }
                        data.HtmlAttributes.AddOrSkipIfExists("type", "checkbox");
                        if (data.Options.CustomOptions == null) data.Options.CustomOptions = member.Member.GetCustomAttribute<CheckBoxAttribute>();
                        break;
                    }
                case InnerTemplateType.Radio:
                    {
                        if (data.Options.CustomOptions == null) data.Options.CustomOptions = member.Member.GetCustomAttribute<RadioAttribute>();
                        break;
                    }
                case InnerTemplateType.RadioList:
                    {
                        if (data.Options.CustomOptions == null) data.Options.CustomOptions = member.Member.GetCustomAttribute<RadioAttribute>();
                        ProcessSelect<TValue>(baseType, metadata, data);
                        break;
                    }
                case InnerTemplateType.File:
                    {
                        HttpPostedFileBaseFileExtensionsAttribute.Resolve(metadata, data.HtmlAttributes);
                        if (data.Options.CustomOptions == null) data.Options.CustomOptions = "Choose";
                        break;
                    }
                case InnerTemplateType.ReadOnly:
                    {
                        if (!String.IsNullOrEmpty(metadata.DisplayFormatString))
                        {
                            data.Value = String.Format("{" + metadata.DisplayFormatString + "}", model);
                        }
                        else if (data.Options.SelectList != null && data.Options.SelectList.Any(x => x.Value == data.Value?.ToString()))
                        {
                            data.Value = data.Options.SelectList.First(x => x.Value == data.Value.ToString()).Text;
                        }
                        else if (data.Options.SelectList != null && data.Options.SelectList.Any(x => x.Value == data.Value.ToString()))
                        {
                            data.Value = data.Options.SelectList.First(x => x.Value == data.Value.ToString()).Text;
                        }
                        break;
                    }
            }

            if (data.HtmlAttributes.ContainsKey("Name"))
            {
                data.Name = data.HtmlAttributes["Name"].ToString();
            }

#if NET_4X
            return html.Partial($"FieldTemplates/{data.Options.TemplateType}", data).AsHtmlContent();
#else
            return html.PartialAsync($"FieldTemplates/{data.Options.TemplateType}", data);
#endif
        }

        private static void ProcessSelect<TValue>(Type baseType, ModelMetadata metadata, FieldInnerTemplateModel data)
        {
            if (data.Options.SelectList != null)
            {
                if (metadata.ModelType.IsGenericType && metadata.ModelType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    data.HtmlAttributes.Add("multiple", "multiple");
                    var values = new List<string>();
                    var loop = (data.Value as IEnumerable).GetEnumerator();
                    while (loop.MoveNext()) values.Add(loop.Current.ToString());
                    data.Options.SelectList = data.Options.SelectList.ToSelectList(v => v.Value, t => t.Text, values);
                }
                else
                {
                    if (data.Options.SelectList.Any(o => o.Group != null))
                        data.Options.SelectList = data.Options.SelectList.ToSelectList(v => v.Value, t => t.Text, g => g.Group.Name, data.Value);
                    else
                        data.Options.SelectList = data.Options.SelectList.ToSelectList(v => v.Value, t => t.Text, data.Value);
                }
            }
            else if (baseType == typeof(Enum))
            {
                Type enumType = Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType;
                var values = EnumExtensions.GetValues(enumType).Cast<TValue>();
                var modelValues = data.Value == null ? new string[0] : data.Value.ToString().Split(',');
                for (var i = 0; i < modelValues.Count(); i++) modelValues[i] = modelValues[i].Trim();

                data.Options.SelectList =
                    from v in values
                    select new SelectListItem
                    {
                        Text = EnumExtensions.GetEnumDescription(v),
                        Value = v.ToString(),
                        Selected = modelValues.Contains(v.ToString())
                    };
                var flags = enumType.GetCustomAttribute<FlagsAttribute>();
                if (flags != null && !data.HtmlAttributes.ContainsKey("multiple")) data.HtmlAttributes.Add("multiple", "multiple");
            }

            data.Options.SelectList = EmptyItemAttribute.Resolve(metadata, data.Options.SelectList, SingleEmptyItem);
            data.Options.SelectList = RemoveItemAttribute.Resolve(metadata, data.Options.SelectList);
        }


    }
}