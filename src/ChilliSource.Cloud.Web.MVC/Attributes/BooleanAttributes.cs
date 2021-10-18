using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Core.Extensions;

#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Validation attribute that demands that a boolean value must be true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MustBeTrueAttribute : ValidationAttribute
#if NET_4X
        , IClientValidatable
#else
        , IClientModelValidator
#endif
    {
        public override bool IsValid(object value)
        {
            return value != null && value is bool && (bool)value;
        }

#if NET_4X
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            yield return new ModelClientValidationRule()
            {
                ErrorMessage = FormatErrorMessage(metadata.DisplayName),
                ValidationType = "mustbetrue"
            };
        }
#else
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.AddOrSkipIfExists("data-val", "true");
            context.Attributes.AddOrSkipIfExists("data-val-mustbetrue", FormatErrorMessage(context.ModelMetadata.DisplayName));
        }
#endif
    }
}