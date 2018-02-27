
using ChilliSource.Core.Extensions; using ChilliSource.Cloud.Core;
using System;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Contains model binding methods for ChilliSource.Cloud.CoreShortGuid object.
    /// </summary>
    public class ShortGuidBinder : IModelBinder
    {
        /// <summary>
        /// Binds the model for ChilliSource.Cloud.Core.ShortGuid object.
        /// </summary>
        /// <param name="controllerContext">The context within which the controller operates.</param>
        /// <param name="bindingContext">The context within which the model is bound.</param>
        /// <returns>The bound ChilliSource.Cloud.Core.ShortGuid object.</returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = BinderHelper.GetValue(bindingContext);

            if (value == null) return null;

            if (value is ShortGuid) return value;

            if (value is Guid) return ((Guid)value).ToShortGuid();

            if (value is string) return new ShortGuid((string)value);

            return null;
        }
    }
}