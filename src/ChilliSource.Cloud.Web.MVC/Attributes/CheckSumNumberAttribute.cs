using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    /// Base class for CheckSum attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class CheckSumNumberAttribute : ValidationAttribute
#if NET_4X
        , IClientValidatable
#else
        , IClientModelValidator
#endif
    {
        /// <summary>
        /// Description of CheckSumType to be used in client validation rules.
        /// </summary>
        public string CheckSumType { get; private set; }

        protected CheckSumNumberAttribute(string checkSumType)
        {
            CheckSumType = checkSumType;
        }

#if NET_4X
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule()
                           {
                               ErrorMessage = FormatErrorMessage(metadata.DisplayName),
                               ValidationType = "checksum"
                           };

            rule.ValidationParameters.Add("checksumtype", this.CheckSumType);

            yield return rule;
        }
#else    
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.AddOrSkipIfExists("data-val", "true");
            context.Attributes.AddOrSkipIfExists("data-val-checksum", FormatErrorMessage(context.ModelMetadata.DisplayName));
            context.Attributes.AddOrSkipIfExists("data-val-checksum-checksumtype", this.CheckSumType);
        }
#endif        

        protected static string RemoveWhitespacesInBetween(string number)
        {
            return number.Replace(" ", "");
        }
    }
}