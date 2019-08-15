using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

#if NET_4X
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
#endif

namespace ChilliSource.Cloud.Web.MVC
{
#if NET_4X
    public class BaseWebController : AsyncController
    {
        protected override ITempDataProvider CreateTempDataProvider()
        {
            return new CookieTempDataProvider(useEncryption: true);
        }

        public BaseWebController() { }

        public ViewNamingConvention ViewNamingConvention { get; set; }

        [DebuggerNonUserCode]
        public IServiceCallerSyntax<T> ServiceCall<T>(Func<ServiceResult<T>> action)
        {
            return (new ServiceCaller<T>(this)).SetAction(action);
        }

        [DebuggerNonUserCode]
        public IServiceCallerSyntax<PagedList<T>> ServiceCall<T>(Func<PagedList<T>> action)
        {
            Func<ServiceResult<PagedList<T>>> wrappedAction = () => ServiceResult<PagedList<T>>.AsSuccess(action());

            return (new ServiceCaller<PagedList<T>>(this)).SetAction(wrappedAction);
        }

        [DebuggerNonUserCode]
        public IServiceCallerAsyncSyntax<T> ServiceCall<T>(Func<Task<ServiceResult<T>>> action)
        {
            return (new ServiceCallerAsync<T>(this)).SetAction(action);
        }

        [DebuggerNonUserCode]
        public IServiceCallerAsyncSyntax<PagedList<T>> ServiceCall<T>(Func<Task<PagedList<T>>> action)
        {
            Func<Task<ServiceResult<PagedList<T>>>> wrappedAction = async () => ServiceResult<PagedList<T>>.AsSuccess(await action());

            return (new ServiceCallerAsync<PagedList<T>>(this)).SetAction(wrappedAction);
        }

        //Exposing protected method internally
        internal new ActionResult View(object model)
        {
            return base.View(model);
        }

        //Exposing protected method internally
        internal new ActionResult View(string viewName, object model)
        {
            return base.View(viewName, model);
        }
    }

#endif

    public enum ViewNamingConvention
    {
        Default = 0,
        ControllerPrefix
    }

    public interface IServiceCallerSyntax<T>
    {
        IServiceCallerSyntax<T> OnSuccess(Func<ActionResult> onSuccess);
        IServiceCallerSyntax<T> OnSuccess(Func<T, ActionResult> onSuccess);
        IServiceCallerSyntax<T> OnServiceSuccess(Func<ServiceResult<T>, ActionResult> onSuccess);
        IServiceCallerSyntax<T> OnFailure(Func<ActionResult> onFailure);
        IServiceCallerSyntax<T> OnFailure(Func<T, ActionResult> onFailure);
        IServiceCallerSyntax<T> OnServiceFailure(Func<ServiceResult<T>, ActionResult> onFailure);
        IServiceCallerSyntax<T> Always(Func<ActionResult> always);
        IServiceCallerSyntax<T> Always(Func<T, ActionResult> always);
        ActionResult Call();
    }

    [DebuggerNonUserCode]
    internal class ServiceCaller<T> : IServiceCallerSyntax<T>
    {
        private Func<ServiceResult<T>> _action;

        private Func<ServiceResult<T>, ActionResult> _onFailure;

        private Func<ServiceResult<T>, ActionResult> _onSuccess;

#if NET_4X
        private readonly BaseWebController _controller;
#else
        private readonly Controller _controller;
#endif

        private bool IgnoreModelState
        {
            get { return (_controller.HttpContext.Items["ServiceCaller_IsModelStateEvaluated"] as bool?).GetValueOrDefault(false); }
            set { _controller.HttpContext.Items["ServiceCaller_IsModelStateEvaluated"] = true; }
        }

        private ViewNamingConvention NamingConvention
        {
            get
            {
                if (Enum.TryParse<ViewNamingConvention>(_controller.HttpContext.Items["ServiceCaller_ViewNamingConvention"] as string, out var convention))
                    return convention;
                return ViewNamingConvention.Default;
            }
        }

#if NET_4X
        public ServiceCaller(BaseWebController controller)
         {
            _controller = controller;

            //default action for success;
            if (controller.ViewNamingConvention == ViewNamingConvention.Default)
            {
                this.OnServiceSuccess((response) => _controller.View(response.Result));
            }
            else if (controller.ViewNamingConvention == ViewNamingConvention.ControllerPrefix)
            {
                var viewname = controller.RouteData.Values["controller"].ToString() + controller.RouteData.Values["action"].ToString();
                this.OnServiceSuccess((response) => _controller.View(viewname, response.Result));
            }

            //default action for failure;
            _onFailure = _onSuccess;
        }
#else
        public ServiceCaller(Controller controller)
        {
            _controller = controller;

            //default action for success;
            if (this.NamingConvention == ViewNamingConvention.Default)
            {
                this.OnServiceSuccess((response) => _controller.View(response.Result));
            }
            else if (this.NamingConvention == ViewNamingConvention.ControllerPrefix)
            {
                var viewname = controller.RouteData.Values["controller"].ToString() + controller.RouteData.Values["action"].ToString();
                this.OnServiceSuccess((response) => _controller.View(viewname, response.Result));
            }

            //default action for failure;
            _onFailure = _onSuccess;
        }
#endif

        [DebuggerNonUserCode]
        public ServiceCaller<T> SetAction(Func<ServiceResult<T>> action)
        {
            _action = action;
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerSyntax<T> OnSuccess(Func<ActionResult> onSuccess)
        {
            _onSuccess = T => onSuccess();
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerSyntax<T> OnSuccess(Func<T, ActionResult> onSuccess)
        {
            _onSuccess = (ServiceResult<T> result) => onSuccess(result.Result);
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerSyntax<T> OnServiceSuccess(Func<ServiceResult<T>, ActionResult> onSuccess)
        {
            _onSuccess = onSuccess;
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerSyntax<T> OnFailure(Func<ActionResult> onFailure)
        {
            _onFailure = T => onFailure();
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerSyntax<T> OnFailure(Func<T, ActionResult> onFailure)
        {
            _onFailure = (ServiceResult<T> result) => onFailure(result.Result);
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerSyntax<T> OnServiceFailure(Func<ServiceResult<T>, ActionResult> onFailure)
        {
            _onFailure = onFailure;
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerSyntax<T> Always(Func<ActionResult> always)
        {
            _onSuccess = T => always();
            _onFailure = T => always();
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerSyntax<T> Always(Func<T, ActionResult> always)
        {
            this.OnSuccess(always);
            _onFailure = _onSuccess;
            return this;
        }

        [DebuggerNonUserCode]
        public ActionResult Call()
        {
            if (_action == null ||
               _onSuccess == null ||
               _onFailure == null)
            {
                throw new ApplicationException("You need to set up the service call, and either Always or OnSuccess and OnFailure actions");
            }

            if (!_controller.ModelState.IsValid && !IgnoreModelState)
            {
                this.IgnoreModelState = true;
                return _onFailure(ServiceResult<T>.AsError(result: default(T)));
            }

            var response = _action();
            if (!response.Success)
            {
                response.AddToModelState(_controller);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
#if NET_4X
                    return new HttpNotFoundResult(response.Error);
#else
                    return _controller.NotFound(response.Error);
#endif

                }
                this.IgnoreModelState = true;
                return _onFailure(response);
            }

            return _onSuccess(response);
        }
    }

    public interface IServiceCallerAsyncSyntax<T>
    {
        IServiceCallerAsyncSyntax<T> OnSuccess(Func<Task<ActionResult>> onSuccess);
        IServiceCallerAsyncSyntax<T> OnSuccess(Func<T, Task<ActionResult>> onSuccess);
        IServiceCallerAsyncSyntax<T> OnServiceSuccess(Func<ServiceResult<T>, Task<ActionResult>> onSuccess);
        IServiceCallerAsyncSyntax<T> OnFailure(Func<Task<ActionResult>> onFailure);
        IServiceCallerAsyncSyntax<T> OnFailure(Func<T, Task<ActionResult>> onFailure);
        IServiceCallerAsyncSyntax<T> OnServiceFailure(Func<ServiceResult<T>, Task<ActionResult>> onFailure);
        IServiceCallerAsyncSyntax<T> Always(Func<Task<ActionResult>> always);
        IServiceCallerAsyncSyntax<T> Always(Func<T, Task<ActionResult>> always);

        Task<ActionResult> Call();
    }

    // *** Contains methods which expect ActionResult instead of Task<ActionResult> because all controller.View(...) methods simply return ActionResult.
    [DebuggerNonUserCode]
    public static class ServiceCallerAsyncExtensions
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        #region ActionResult methods
        [DebuggerNonUserCode]
        public static IServiceCallerAsyncSyntax<T> OnSuccess<T>(this IServiceCallerAsyncSyntax<T> caller, Func<ActionResult> onSuccess)
        {
            return caller.OnSuccess(async () => onSuccess());
        }

        [DebuggerNonUserCode]
        public static IServiceCallerAsyncSyntax<T> OnSuccess<T>(this IServiceCallerAsyncSyntax<T> caller, Func<T, ActionResult> onSuccess)
        {
            return caller.OnSuccess(async (r) => onSuccess(r));
        }

        [DebuggerNonUserCode]
        public static IServiceCallerAsyncSyntax<T> OnServiceSuccess<T>(this IServiceCallerAsyncSyntax<T> caller, Func<ServiceResult<T>, ActionResult> onSuccess)
        {
            return caller.OnServiceSuccess(async (r) => onSuccess(r));
        }

        [DebuggerNonUserCode]
        public static IServiceCallerAsyncSyntax<T> OnFailure<T>(this IServiceCallerAsyncSyntax<T> caller, Func<ActionResult> onFailure)
        {
            return caller.OnFailure(async () => onFailure());
        }

        [DebuggerNonUserCode]
        public static IServiceCallerAsyncSyntax<T> OnFailure<T>(this IServiceCallerAsyncSyntax<T> caller, Func<T, ActionResult> onFailure)
        {
            return caller.OnFailure(async (r) => onFailure(r));
        }

        [DebuggerNonUserCode]
        public static IServiceCallerAsyncSyntax<T> OnServiceFailure<T>(this IServiceCallerAsyncSyntax<T> caller, Func<ServiceResult<T>, ActionResult> onFailure)
        {
            return caller.OnServiceFailure(async (r) => onFailure(r));
        }

        [DebuggerNonUserCode]
        public static IServiceCallerAsyncSyntax<T> Always<T>(this IServiceCallerAsyncSyntax<T> caller, Func<ActionResult> always)
        {
            return caller.Always(async () => always());
        }

        [DebuggerNonUserCode]
        public static IServiceCallerAsyncSyntax<T> Always<T>(this IServiceCallerAsyncSyntax<T> caller, Func<T, ActionResult> always)
        {
            return caller.Always(async (r) => always(r));
        }
        #endregion
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }

    [DebuggerNonUserCode]
    internal class ServiceCallerAsync<T> : IServiceCallerAsyncSyntax<T>
    {
        private Func<Task<ServiceResult<T>>> _action;

        private Func<ServiceResult<T>, Task<ActionResult>> _onFailure;

        private Func<ServiceResult<T>, Task<ActionResult>> _onSuccess;

#if NET_4X
        private readonly BaseWebController _controller;
#else
        private readonly Controller _controller;
#endif

        private bool IgnoreModelState
        {
            get { return (_controller.HttpContext.Items["ServiceCaller_IsModelStateEvaluated"] as bool?).GetValueOrDefault(false); }
            set { _controller.HttpContext.Items["ServiceCaller_IsModelStateEvaluated"] = value; }
        }

#if NET_4X
        public ServiceCallerAsync(BaseWebController controller)
         {
            _controller = controller;

            //default action for success;
            if (controller.ViewNamingConvention == ViewNamingConvention.Default)
            {
                this.OnServiceSuccess((response) => Task.FromResult<ActionResult>(_controller.View(response.Result)));
            }
            else if (controller.ViewNamingConvention == ViewNamingConvention.ControllerPrefix)
            {
                var viewname = controller.RouteData.Values["controller"].ToString() + controller.RouteData.Values["action"].ToString();
                this.OnServiceSuccess((response) => Task.FromResult<ActionResult>(_controller.View(viewname, response.Result)));
            }

            //default action for failure;
            _onFailure = _onSuccess;
        }
#else
        public ServiceCallerAsync(Controller controller)
        {
            _controller = controller;
            this.OnServiceSuccess((response) => Task.FromResult<ActionResult>(_controller.View(response.Result)));
            //default action for failure;
            _onFailure = _onSuccess;
        }
#endif

        [DebuggerNonUserCode]
        public ServiceCallerAsync<T> SetAction(Func<Task<ServiceResult<T>>> action)
        {
            _action = action;
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerAsyncSyntax<T> OnSuccess(Func<Task<ActionResult>> onSuccess)
        {
            _onSuccess = T => onSuccess();
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerAsyncSyntax<T> OnSuccess(Func<T, Task<ActionResult>> onSuccess)
        {
            _onSuccess = (ServiceResult<T> result) => onSuccess(result.Result);
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerAsyncSyntax<T> OnServiceSuccess(Func<ServiceResult<T>, Task<ActionResult>> onSuccess)
        {
            _onSuccess = onSuccess;
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerAsyncSyntax<T> OnFailure(Func<Task<ActionResult>> onFailure)
        {
            _onFailure = T => onFailure();
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerAsyncSyntax<T> OnFailure(Func<T, Task<ActionResult>> onFailure)
        {
            _onFailure = (ServiceResult<T> result) => onFailure(result.Result);
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerAsyncSyntax<T> OnServiceFailure(Func<ServiceResult<T>, Task<ActionResult>> onFailure)
        {
            _onFailure = onFailure;
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerAsyncSyntax<T> Always(Func<Task<ActionResult>> always)
        {
            _onSuccess = T => always();
            _onFailure = T => always();
            return this;
        }

        [DebuggerNonUserCode]
        public IServiceCallerAsyncSyntax<T> Always(Func<T, Task<ActionResult>> always)
        {
            this.OnSuccess(always);
            _onFailure = _onSuccess;
            return this;
        }

        [DebuggerNonUserCode]
        public async Task<ActionResult> Call()
        {
            if (_action == null ||
               _onSuccess == null ||
               _onFailure == null)
            {
                throw new ApplicationException("You need to set up the service call, and either Always or OnSuccess and OnFailure actions");
            }

            if (!_controller.ModelState.IsValid && !IgnoreModelState)
            {
                this.IgnoreModelState = true;
                return await _onFailure(ServiceResult<T>.AsError(result: default(T)));
            }

            var response = await _action();
            if (!response.Success)
            {
                response.AddToModelState(_controller);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
#if NET_4X
                    return new HttpNotFoundResult(response.Error);
#else
                    return _controller.NotFound(response.Error);
#endif
                }
                this.IgnoreModelState = true;
                return await _onFailure(response);
            }

            return await _onSuccess(response);
        }
    }
}