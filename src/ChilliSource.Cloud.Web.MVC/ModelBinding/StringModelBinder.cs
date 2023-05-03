#if !NET_4X
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Web.MVC.ModelBinding
{

    public class TrimStringJsonConverter : JsonConverter<string>
    {
        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.Value != null)
            {
                if (reader.Value is string) return (reader.Value as string).Trim();
                return reader.Value.ToString();
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, string value, Newtonsoft.Json.JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }

    //Baseed from https://stackoverflow.com/questions/47079791/asp-net-web-api-core-complex-data-model-binder-to-trim-strings
    public class StringModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(string))
                return Task.CompletedTask;

            var modelName = bindingContext.ModelName;
            if (String.IsNullOrWhiteSpace(modelName))
                return Task.CompletedTask;

            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.Result = ModelBindingResult.Success(
                valueProviderResult.FirstValue.TrimAndNullIfWhiteSpace());

            return Task.CompletedTask;
        }
    }

    static class NormalizeString
    {
        public static string TrimAndNullIfWhiteSpace(this string text) =>
           string.IsNullOrWhiteSpace(text)
           ? null
           : text.Trim();
    }

    public class StringModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(string))
                return new StringModelBinder();

            return null;
        }
    }

    public static class StringModelBinderServiceCollectionExtensions
    {

        public static MvcOptions AddStringModelBinderProvider(this MvcOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.ModelBinderProviders.InsertStringModelBinderProvider();
            return options;
        }

        public static void InsertStringModelBinderProvider(this IList<IModelBinderProvider> modelBinderProviders)
        {
            if (modelBinderProviders == null)
                throw new ArgumentNullException(nameof(modelBinderProviders));

            var providerToInsert = new StringModelBinderProvider();

            var index = modelBinderProviders.FirstIndexOfOrDefault(i => i is SimpleTypeModelBinderProvider);

            if (index >= 0)
                modelBinderProviders.Insert(0, providerToInsert);
            else
                modelBinderProviders.Add(providerToInsert);
        }

        private static int FirstIndexOfOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var result = 0;

            foreach (var item in source)
            {
                if (predicate(item))
                    return result;

                result++;
            }

            return -1;
        }
    }
}
#endif