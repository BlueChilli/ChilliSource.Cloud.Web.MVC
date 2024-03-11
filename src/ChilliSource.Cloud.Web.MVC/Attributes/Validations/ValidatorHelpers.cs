using ChilliSource.Core.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ChilliSource.Cloud.Web.MVC
{
    public static class ValidatorHelper
    {
        public static bool TryValidateObject(object obj, out string? error)
        {
            error = null;

            if (obj == null) return true;

            var results = new List<ValidationResult>();
            var valid = Validator.TryValidateObject(obj, new ValidationContext(obj), results, validateAllProperties: true);
            if (valid) return true;

            error = results.Select(x => x.ErrorMessage).ToDelimitedString(", ");
            return false;
        }

    }
}
