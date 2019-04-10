#if !NET_4X
/* 
* Based on https://github.com/aspnet/AspNetCore.Docs/tree/master/aspnetcore/mvc/models/file-uploads/sample/FileUploadSample
* License (MIT): https://github.com/aspnet/AspNetCore.Docs/blob/master/LICENSE-CODE
*/

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Web.MVC
{
    public class MultipartContentParser
    {
        private FormOptions _formOptions;

        public MultipartContentParser(IOptions<FormOptions> formOptions)
            : this(formOptions.Value) { }

        public MultipartContentParser(FormOptions formOptions)
        {
            _formOptions = formOptions;
        }

        public Task<IMultipartResult<T>> ParseRequestAsync<T>(HttpRequest request, Func<IMultipartHttpFile, CancellationToken, Task<T>> fileTask, CancellationToken cancellationToken)
        {
            var contentType = MediaTypeHeaderValue.Parse(request.ContentType);
            return ParseAsync(contentType, request.Body, fileTask, cancellationToken);
        }

        public async Task<IMultipartResult<T>> ParseAsync<T>(MediaTypeHeaderValue contentType, Stream bodyStream, Func<IMultipartHttpFile, CancellationToken, Task<T>> fileTask, CancellationToken cancellationToken)
        {
            List<T> fileResults = new List<T>();
            var formAccumulator = new KeyValueAccumulator();

            var boundary = GetBoundary(contentType, _formOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, bodyStream);

            var section = await reader.ReadNextSectionAsync(cancellationToken);
            while (section != null)
            {
                using (var sectionStream = section.Body)
                {
                    ContentDispositionHeaderValue contentDisposition;
                    var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

                    if (hasContentDispositionHeader)
                    {
                        if (HasFileContentDisposition(contentDisposition))
                        {
                            var fileName = HeaderUtilities.RemoveQuotes(!StringSegment.IsNullOrEmpty(contentDisposition.FileName) ? contentDisposition.FileName : contentDisposition.FileNameStar);
                            var httpFile = new MultipartHttpFile(fileName.ToString(), section.ContentType, sectionStream);

                            var actionResult = await fileTask(httpFile, cancellationToken);
                            fileResults.Add(actionResult);
                        }
                        else if (HasFormDataContentDisposition(contentDisposition))
                        {
                            var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                            var encoding = GetEncoding(section);
                            using (var streamReader = new StreamReader(sectionStream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                            {
                                var value = await streamReader.ReadToEndAsync();
                                if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = String.Empty;
                                }
                                formAccumulator.Append(key.ToString(), value);

                                if (formAccumulator.ValueCount > _formOptions.ValueCountLimit)
                                {
                                    throw new InvalidDataException($"Form key count limit {_formOptions.ValueCountLimit} exceeded.");
                                }
                            }
                        }
                    }
                }

                section = await reader.ReadNextSectionAsync(cancellationToken);
            }

            return new MultipartResult<T>(formAccumulator.GetResults(), fileResults);
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            MediaTypeHeaderValue mediaType;
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }

        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).ToString();
            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary;
        }

        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("Multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && StringSegment.IsNullOrEmpty(contentDisposition.FileName)
                   && StringSegment.IsNullOrEmpty(contentDisposition.FileNameStar);
        }

        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            return contentDisposition != null
                   && contentDisposition.DispositionType.Equals("form-data")
                   && (!StringSegment.IsNullOrEmpty(contentDisposition.FileName)
                       || !StringSegment.IsNullOrEmpty(contentDisposition.FileNameStar));
        }
    }

    public interface IMultipartResult<T>
    {
        Dictionary<string, StringValues> FormResults { get; }
        List<T> FileResults { get; }
    }

    public interface IMultipartHttpFile
    {
        string ContentType { get; }

        string FileName { get; }

        Stream InputStream { get; }
    }

    internal class MultipartResult<T> : IMultipartResult<T>
    {
        protected MultipartResult() { }
        public MultipartResult(Dictionary<string, StringValues> formResults, List<T> fileResults)
        {
            FormResults = formResults;
            FileResults = fileResults;
        }

        public virtual Dictionary<string, StringValues> FormResults { get; }
        public virtual List<T> FileResults { get; }
    }

    public static class MultipartResultExtensions
    {
        public static FormValueProvider GetFormValueProvider<T>(this IMultipartResult<T> result, CultureInfo culture)
        {
            return new FormValueProvider(BindingSource.Form, new FormCollection(result.FormResults), culture);
        }
    }

    internal class MultipartHttpFile : IMultipartHttpFile
    {
        public MultipartHttpFile(string fileName, string contentType, Stream stream)
        {
            FileName = fileName;
            ContentType = contentType;
            InputStream = stream;
        }

        public string ContentType { get; }

        public string FileName { get; }

        public Stream InputStream { get; }
    }
}
#endif