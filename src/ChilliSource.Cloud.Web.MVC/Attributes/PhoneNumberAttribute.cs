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

        public static void Resolve(MemberExpression member, ModelMetadata metadata, RouteValueDictionary attributes)
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
