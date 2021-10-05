#if NET_4X
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using ChilliSource.Cloud.Web;
using ChilliSource.Cloud.Core.Phone;
using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using PhoneNumbers;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Validates phone fields. 
    /// Default is 10 maximumDigits (Australian).
    /// Minimum length defaults to 10.
    /// Use PhoneNumber(15) for international numbers. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PhoneNumberAttribute : StringLengthAttribute, IPropertyBinderProvider
    {
        private class Binder : IPropertyBinder
        {
            public void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, ValueProviderResult valueProviderResult)
            {
                if (valueProviderResult == null)
                    return;

                var sValue = valueProviderResult.AttemptedValue;
                if (sValue != null)
                {
                    sValue = sValue.ToNumeric();
                }

                propertyDescriptor.SetValue(bindingContext.Model, sValue);
            }
        }

        public PhoneNumberAttribute(string region, int maximumDigits = 10, params PhoneNumberType[] phoneTypesToCheck)
            : base(maximumDigits)
        {
            this.MinimumLength = 10;
            this.MaximumDigits = maximumDigits;
            this.Region = region;
            this.PhoneTypesToCheck = phoneTypesToCheck?.Length > 0 ? phoneTypesToCheck
                                        : new PhoneNumberType[] { PhoneNumberType.FIXED_LINE_OR_MOBILE, PhoneNumberType.MOBILE, PhoneNumberType.FIXED_LINE };
        }

        IPropertyBinder IPropertyBinderProvider.CreateBinder()
        {
            return new PhoneNumberAttribute.Binder();
        }

        /// <summary>
        /// Maximum number of digits. Defaults to 10.
        /// </summary>
        public int MaximumDigits { get; private set; }

        public string Region { get; set; }
        public PhoneNumberType[] PhoneTypesToCheck { get; set; }

        public override string FormatErrorMessage(string name)
        {
            return String.Format("The {0} field contains an invalid phone number.", name);
        }

        public static void Resolve(MemberExpression member, ModelMetadata metadata, IDictionary<string, object> attributes)
        {
            var phoneNumberAttribute = member.Member.GetAttribute<PhoneNumberAttribute>(false);
            if (phoneNumberAttribute != null || metadata.DataTypeName == "PhoneNumber")
            {
                attributes.AddOrSkipIfExists("type", "tel");
            }
        }

        public override bool IsValid(object value)
        {
            var s = value as string;
            if (String.IsNullOrEmpty(s)) return true;
            if (s != null && s.Length <= MaximumDigits && s.Length >= MinimumLength &&
                s.IsValidPhoneNumber(this.Region, this.PhoneTypesToCheck))
            {
                return base.IsValid(value);
            }

            return false;
        }
    }
}
#else
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Core.Phone;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Validates phone fields. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PhoneNumberAttribute : ValidationAttribute, IModelBinderProvider, IDisplayMetadataProvider
    {
        public PhoneNumberAttribute(string region, params PhoneNumberType[] phoneTypesToCheck)
        {
            this.Region = region;
            this.PhoneTypesToCheck = phoneTypesToCheck?.Length > 0 ? phoneTypesToCheck
                                        : new PhoneNumberType[] { PhoneNumberType.FIXED_LINE_OR_MOBILE, PhoneNumberType.MOBILE, PhoneNumberType.FIXED_LINE };
        }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            context.DisplayMetadata.AdditionalValues.Add("ChilliSource:PhoneNumberAttribute", true);
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.AdditionalValues.ContainsKey("ChilliSource:PhoneNumberAttribute"))
            {
                return new BinderTypeModelBinder(typeof(PhoneNumberBinder));
            }

            return null;
        }

        public string Region { get; set; }
        public PhoneNumberType[] PhoneTypesToCheck { get; set; }

        public override string FormatErrorMessage(string name)
        {
            return String.Format("The {0} field contains an invalid phone number.", name);
        }

        public static void Resolve(MemberExpression member, ModelMetadata metadata, IDictionary<string, object> attributes)
        {
            var phoneNumberAttribute = member.Member.GetAttribute<PhoneNumberAttribute>(false);
            if (phoneNumberAttribute != null || metadata.DataTypeName == "PhoneNumber")
            {
                attributes.AddOrSkipIfExists("type", "tel");
            }
        }

        public override bool IsValid(object value)
        {
            var s = value as string;
            if (String.IsNullOrEmpty(s)) return true;
            return s.IsValidPhoneNumber(this.Region, this.PhoneTypesToCheck);
        }
    }

    public class PhoneNumberBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult != null
                && valueProviderResult != ValueProviderResult.None
                && valueProviderResult.FirstValue is string str
                && !String.IsNullOrEmpty(str))
            {
                bindingContext.Result = ModelBindingResult.Success(str.ToNumeric());
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

    }
}

#endif
