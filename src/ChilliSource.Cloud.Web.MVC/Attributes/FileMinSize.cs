#if NET_4X
#else

using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Validates the Min size for uploaded files. This must be applied on IFormFile fields.
    /// </summary>
    public class FileMinSizeAttribute : ValidationAttribute
    {
        public FileMinSizeAttribute()
        {
            ErrorMessage = "The {0} field: The size of the file selected must be greater than {1}.";
        }

        public FileMinSizeAttribute(long minSizeinBytes) : base()
        {
            MinSize = minSizeinBytes;
        }

        /// <summary>
        /// Min size in bytes
        /// </summary>
        public long MinSize { get; private set; }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(ErrorMessage, name, FormatBytes(MinSize));
        }

        private static string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }

        public override bool IsValid(object value)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                return file.Length > MinSize;
            }

            return true;
        }

        //public void AddValidation(ClientModelValidationContext context)
        //{
        //    context.Attributes.AddOrSkipIfExists("data-val", "true");
        //    context.Attributes.AddOrSkipIfExists("data-val-filemaxsize", FormatErrorMessage(context.ModelMetadata.DisplayName));
        //    context.Attributes.AddOrSkipIfExists("data-val-filemaxsize-filemaxsize", this.MaxSize.ToString());
        //}
    }
}
#endif