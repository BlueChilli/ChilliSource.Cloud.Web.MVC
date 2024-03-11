using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace ChilliSource.Cloud.Web.MVC
{
    public class ValidateObjectAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(value, null, null);

            var asEnumerable = value as IEnumerable;
            if (asEnumerable != null)
            {
                foreach (var enumObj in asEnumerable)
                {
                    if (enumObj != null)
                    {
                        var enumContext = new ValidationContext(enumObj, null, null);
                        Validator.TryValidateObject(enumObj, enumContext, results, true);
                    }
                }
            }
            else
            {
                Validator.TryValidateObject(value, context, results, true);
            }

            if (results.Count != 0)
            {
                var compositeResults = new CompositeValidationResult(String.Format("Validation for {0} failed!", validationContext.DisplayName));
                results.ForEach(compositeResults.AddResult);

                return compositeResults;
            }

            return ValidationResult.Success;
        }
    }

    public class CompositeValidationResult : ValidationResult
    {
        private readonly List<ValidationResult> _results = new List<ValidationResult>();

        public IEnumerable<ValidationResult> Results
        {
            get
            {
                return _results;
            }
        }

        public CompositeValidationResult(string errorMessage) : base(errorMessage) { }
        public CompositeValidationResult(string errorMessage, IEnumerable<string> memberNames) : base(errorMessage, memberNames) { }
        protected CompositeValidationResult(ValidationResult validationResult) : base(validationResult) { }

        public void AddResult(ValidationResult validationResult)
        {
            _results.Add(validationResult);
        }
    }
}
