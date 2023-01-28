using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Core.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Validation attribute that demands that a boolean value must be true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value != null && value is bool && (bool)value;
        }
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.AddOrSkipIfExists("data-val", "true");
            context.Attributes.AddOrSkipIfExists("data-val-mustbetrue", FormatErrorMessage(context.ModelMetadata.DisplayName));
        }
    }
}