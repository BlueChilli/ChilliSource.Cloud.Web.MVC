#if !NET_4X
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Web.MVC.ModelBinding
{
    /// https://github.com/sgjsakura/AspNetCore/blob/master/Sakura.AspNetCore.Extensions/Sakura.AspNetCore.Mvc.TagHelpers/FlagsEnumModelBinderProvider.cs
    /// <summary>
    ///     An <see cref="IModelBinderProvider" /> used to provider <see cref="FlagsEnumModelBinder" /> instances.
    /// </summary>
    public class FlagsEnumModelBinderProvider : IModelBinderProvider
    {
        /// <inheritdoc />
        /// <summary>
        ///     Creates a <see cref="IModelBinder" /> based on <see cref="ModelBinderProviderContext" />.
        /// </summary>
        /// <param name="context">The <see cref="ModelBinderProviderContext" />.</param>
        /// <returns>An <see cref="IModelBinder" />.</returns>
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Metadata.IsFlagsEnum ? new FlagsEnumModelBinder() : null;
        }
    }
}
#endif