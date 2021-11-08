﻿#if !NET_4X
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Web.MVC.ModelBinding
{
    /// https://github.com/sgjsakura/AspNetCore/blob/master/Sakura.AspNetCore.Extensions/Sakura.AspNetCore.Mvc.TagHelpers/FlagsEnumModelBinder.cs
    /// <summary>
    ///     Support binding a flags enum value with multiple flag inputs.
    /// </summary>
    public class FlagsEnumModelBinder : IModelBinder
    {
		private static Task CompletedTask => Task.CompletedTask;
        /// <summary>Attempts to bind a model.</summary>
        /// <param name="bindingContext">The <see cref="T:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext" />.</param>
        /// <returns>
        ///     <para>
        ///         A <see cref="T:System.Threading.Tasks.Task" /> which will complete when the model binding process completes.
        ///     </para>
        ///     <para>
        ///         If model binding was successful, the
        ///         <see cref="P:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext.Result" /> should have
        ///         <see cref="P:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingResult.IsModelSet" /> set to <c>true</c>.
        ///     </para>
        ///     <para>
        ///         A model binder that completes successfully should set
        ///         <see cref="P:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext.Result" /> to
        ///         a value returned from
        ///         <see cref="M:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingResult.Success(System.Object)" />.
        ///     </para>
        /// </returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // Only accept enum values
            if (!bindingContext.ModelMetadata.IsFlagsEnum)
                return CompletedTask;

            var provideValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            // Do nothing if there is no actual values
            if (provideValue == ValueProviderResult.None)
                return CompletedTask;

            // Get the real enum type
            var enumType = bindingContext.ModelType;
            enumType = Nullable.GetUnderlyingType(enumType) ?? enumType;

            // Each value self may contains a series of actual values, split it with comma
            var strs = provideValue.Values.SelectMany(s => s.Split(','));

            if (strs.Any(s => String.IsNullOrWhiteSpace(s)))
                return CompletedTask;

            // Convert all items into enum items.
            var actualValues = strs.Select(valueString => Enum.Parse(enumType, valueString));

            // Merge to final result
            var result = actualValues.Aggregate(0, (current, value) => current | (int)value);

            // Convert to Enum object
            var realResult = Enum.ToObject(enumType, result);

            // Result
            bindingContext.Result = ModelBindingResult.Success(realResult);

            return CompletedTask;
        }
    }

}
#endif