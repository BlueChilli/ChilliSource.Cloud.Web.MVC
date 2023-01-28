using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Validates that a DateTime field must have a value greater than another DateTime field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DateGreaterThanAttribute : ValidationAttribute
        , IClientModelValidator
    {
        /// <summary>
        /// (Optional) Set this if you want to use the unaltered DisplayName attribute value in the error message
        /// </summary>
        public string MyProperty { get; private set; }
        /// <summary>
        /// Other DateTime field name.
        /// </summary>
        public string OtherProperty { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherProperty"></param>
        /// <param name="myProperty">Set this if you want to use the unaltered DisplayName attribute value in the error message</param>
        public DateGreaterThanAttribute(string otherProperty, string myProperty = null)
        {
            OtherProperty = otherProperty;
            MyProperty = myProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var otherProperty = validationContext.ObjectInstance.GetType().GetProperty(this.OtherProperty);
            if (otherProperty == null)
                return new ValidationResult(String.Format("unknown property {0}", this.OtherProperty));

            if (value == null || !(value is DateTime))
                return ValidationResult.Success;

            var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

            if (otherValue == null || !(otherValue is DateTime))
                return ValidationResult.Success;

            var to = (DateTime)value;
            var from = (DateTime)otherValue;

            var myProperty = this.MyProperty == null ? null : validationContext.ObjectInstance.GetType().GetProperty(this.MyProperty);
            var myDisplayAttribute = myProperty == null ? null : myProperty.GetCustomAttribute<DisplayNameAttribute>(true);

            var displayName = myDisplayAttribute == null ? validationContext.DisplayName.ToSentenceCase(true) : myDisplayAttribute.DisplayName;
            return to < from ? new ValidationResult(GetErrorMessage(displayName, validationContext.ObjectInstance.GetType())) : ValidationResult.Success;
        }

        private string GetErrorMessage(string displayName, Type modelType)
        {
            var otherProperty = modelType.GetProperty(this.OtherProperty);
            if (otherProperty == null)
                return String.Format("unknown property {0}", this.OtherProperty);

            var displayAttribute = otherProperty.GetAttribute<DisplayNameAttribute>(true);

            var otherPropertyDisplayName = displayAttribute != null ? displayAttribute.DisplayName : OtherProperty.ToSentenceCase(true);

            return String.Format("{0} must be greater than {1}", displayName, otherPropertyDisplayName);
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.AddOrSkipIfExists("data-val", "true");
            context.Attributes.AddOrSkipIfExists("data-val-greaterthan", GetErrorMessage(context.ModelMetadata.DisplayName, context.ModelMetadata.ContainerType));
            context.Attributes.AddOrSkipIfExists("data-val-greaterthan-other", this.OtherProperty);
        }
    }
}