using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

#if NET_4X
using System.Web.Mvc;
using System.Web.Routing;
using System.Web;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Validates the extension of uploaded files.
    /// </summary>
    public class HttpPostedFileExtensionsAttribute : ValidationAttribute, IMetadataAware
#if !NET_4X
        , IClientModelValidator
#endif
    {
        /// <summary>
        /// Validates the extension of uploaded files. Defaults allowedExtensions to jpg, jpeg, png, gif.
        /// </summary>
        public HttpPostedFileExtensionsAttribute()
        {
            Extensions = "jpg, jpeg, png, gif";
            ErrorMessage = "Field {0} is not a valid image type ({1})";
        }

        /// <summary>
        /// Validates the extension of uploaded files.
        /// </summary>
        /// <param name="allowedExtensions">Allowed extensions separated by comma</param>
        public HttpPostedFileExtensionsAttribute(string allowedExtensions)
        {
            Extensions = allowedExtensions.ToLower();
            ErrorMessage = "Field {0} is not one of following valid extensions ({1})";
        }

        public string Extensions { get; set; }

        private List<string> GetExtensions()
        {
            return Extensions.Split(',').Select(s => "." + s.Trim()).ToList();
        }

        public override bool IsValid(object value)
        {
            //IFormFile

#if NET_4X
            var file = value as HttpPostedFileBase;
#else
            var file = value as IFormFile;
#endif

            if (file != null)
            {
                return GetExtensions().Contains(Path.GetExtension(file.FileName).ToLower());
            }

            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            string errorMessage = String.Join(", ", GetExtensions());
            return String.Format(ErrorMessage, name, errorMessage.TrimEnd(' ', ','));
        }

#if NET_4X
        public void OnMetadataCreated(ModelMetadata metadata)
#else
        public void GetDisplayMetadata(DisplayMetadataProviderContext metadata)
#endif
        {
            metadata.AdditionalValues()["FileExtensions"] = Extensions.Replace(" ", "");
        }

        public static void Resolve(ModelMetadata metadata, IDictionary<string, object> attributes)
        {
            if (metadata.AdditionalValues.ContainsKey("FileExtensions"))
            {
                Resolve(metadata.AdditionalValues()["FileExtensions"].ToString(), attributes);
            }
        }

        public static void Resolve(string extensionsData, IDictionary<string, object> attributes)
        {
            attributes["extensions"] = extensionsData.Replace(" ", "");
            var extensions = attributes["extensions"].ToString().Split(',').Select(s => "." + s).ToList();
            var mimeTypes = new List<string>();
            foreach (var extension in extensions)
            {
                var mimeMapping = GlobalConfiguration.Instance.GetMimeMapping();
                var mimeType = mimeMapping.GetMimeType("dummy" + extension);
                if (mimeType.Equals("application/octet-stream", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!mimeTypes.Contains(extension))
                    {
                        mimeTypes.Add(extension);
                    }
                }
                else if (!mimeTypes.Contains(mimeType))
                {
                    mimeTypes.Add(mimeType);
                }
            }
            attributes["accept"] = String.Join(", ", mimeTypes);
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.AddOrSkipIfExists("data-val", "true");
            context.Attributes.AddOrSkipIfExists("data-val-extension", FormatErrorMessage(context.ModelMetadata.DisplayName));
        }

    }
}