﻿using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.DataProtection;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
    public static class FileStorageExtensions
    {
        /// <summary>
        /// Retrieves a file from the remote storage and writes it to output stream determining mime type from source filename
        /// <param name="filename">File name or key for a file in the storage</param>
        /// <param name="attachmentFilename">File name end user will see. This is made file name safe if not already</param>
        /// <param name="isEncrypted">(Optional) Specifies whether the file needs to be decrypted.</param>
        /// <returns>File stream result</returns>
        /// </summary>
        public static FileStreamResult WriteAttachmentContent(this IFileStorage fileStorage, HttpResponse response, string filename, string attachmentFilename = "", StorageEncryptionKeys encryptionKeys = null)
        {
            return TaskHelper.GetResultSafeSync(() => fileStorage.WriteAttachmentContentAsync(response, filename, attachmentFilename, encryptionKeys));
        }

        /// <summary>
        /// Retrieves a file from the remote storage and writes it to output stream determining mime type from source filename
        /// <param name="filename">File name or key for a file in the storage</param>
        /// <param name="attachmentFilename">File name end user will see. This is made file name safe if not already</param>
        /// <param name="isEncrypted">(Optional) Specifies whether the file needs to be decrypted.</param>
        /// <returns>File stream result</returns>
        /// </summary>
        public static async Task<FileStreamResult> WriteAttachmentContentAsync(this IFileStorage fileStorage, HttpResponse response, string filename, string attachmentFilename = "", StorageEncryptionKeys encryptionKeys = null)
        {
            attachmentFilename = attachmentFilename.DefaultTo(filename).ToFilename();
            if (!Path.HasExtension(attachmentFilename)) attachmentFilename = attachmentFilename + Path.GetExtension(filename);
            
            response.Headers["content-disposition"] = string.Format("attachment; filename=\"{0}\"", attachmentFilename);

            var result = await fileStorage.GetContentAsync(filename, encryptionKeys)
                                  .IgnoreContext();
            Stream stream = result.Stream;

            var contentType = String.IsNullOrEmpty(result.ContentType) ? "application/octet-stream" : result.ContentType;
            return new FileStreamResult(stream, contentType);
        }
    }
}
