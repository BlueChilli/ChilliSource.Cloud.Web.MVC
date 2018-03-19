
using ChilliSource.Core.Extensions; using ChilliSource.Cloud.Core;
using System;
using System.Diagnostics;
using System.Net;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Web.MVC
{
    public class BaseWebController : AsyncController
    {
        public ViewNamingConvention ViewNamingConvention { get; set; }


        protected override ITempDataProvider CreateTempDataProvider()
        {
            return new CookieTempDataProvider(useEncryption: true);
        }

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

    public enum ViewNamingConvention
    {
        Default = 0,
        ControllerPrefix
    }

    public interface IServiceCallerSyntax<T>
    {
        IServiceCallerSyntax<T> OnSuccess(Func<ActionResult> onSuccess);
        IServiceCallerSyntax<T> OnSuccess(Func<T, ActionResult> onSuccess);
        IServiceCallerSyntax<T> OnFailure(Func<ActionResult> onFailure);
        IServiceCallerSyntax<T> OnFailure(Func<T, ActionResult> onFailure);
        IServiceCallerSyntax<T> Always(Func<ActionResult> always);
        IServiceCallerSyntax<T> Always(Func<T, ActionResult> always);
        ActionResult Call();
    }

    [DebuggerNonUserCode]
    internal class ServiceCaller<T> : IServiceCallerSyntax<T>
    {
        private Func<ServiceResult<T>> _action;

        private Func<T, ActionResult> _onFailure;

        private Func<T, ActionResult> _onSuccess;

        private readonly BaseWebController _controller;

        private bool IgnoreModelState
        {
            get { return (_controller.HttpContext.Items["ServiceCaller_IsModelStateEvaluated"] as bool?).GetValueOrDefault(false); }
            set { _controller.HttpContext.Items["ServiceCaller_IsModelStateEvaluated"] = true; }
        }

        public ServiceCaller(BaseWebController controller)
        {
            _controller = controller;

            //default action for success;
            if (controller.ViewNamingConvention == ViewNamingConvention.Default)
            {
                this.OnSuccess((response) => _controller.View(response));
            }
            else if (controller.ViewNamingConvention == ViewNamingConvention.ControllerPrefix)
            {
                var viewname = controller.RouteData.Values["controller"].ToString() + controller.RouteData.Values["action"].ToString();
                this.OnSuccess((response) => _controller.View(viewname, response));
            }

            //default action for failure;
            _onFailure = _onSuccess;
        }

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
            _onSuccess = always;
            _onFailure = always;
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

            var responseValue = default(T);
            if (!_controller.ModelState.IsValid && !IgnoreModelState)
            {
                this.IgnoreModelState = true;
                return _onFailure(responseValue);
            }
            var response = _action();
            responseValue = response.Result;
            if (!response.Success)
            {
                response.AddToModelState(_controller);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new HttpNotFoundResult(response.Error);
                }
                this.IgnoreModelState = true;
                return _onFailure(response.Result);
            }

            return _onSuccess(response.Result);
        }
    }

    public interface IServiceCallerAsyncSyntax<T>
    {
        IServiceCallerAsyncSyntax<T> OnSuccess(Func<Task<ActionResult>> onSuccess);
        IServiceCallerAsyncSyntax<T> OnSuccess(Func<T, Task<ActionResult>> onSuccess);
        IServiceCallerAsyncSyntax<T> OnFailure(Func<Task<ActionResult>> onFailure);
        IServiceCallerAsyncSyntax<T> OnFailure(Func<T, Task<ActionResult>> onFailure);
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

        private Func<T, Task<ActionResult>> _onFailure;

        private Func<T, Task<ActionResult>> _onSuccess;

        private readonly BaseWebController _controller;

        private bool IgnoreModelState
        {
            get { return (_controller.HttpContext.Items["ServiceCaller_IsModelStateEvaluated"] as bool?).GetValueOrDefault(false); }
            set { _controller.HttpContext.Items["ServiceCaller_IsModelStateEvaluated"] = true; }
        }

        public ServiceCallerAsync(BaseWebController controller)
        {
            _controller = controller;

            //default action for success;
            if (controller.ViewNamingConvention == ViewNamingConvention.Default)
            {
                this.OnSuccess((response) => _controller.View(response));
            }
            else if (controller.ViewNamingConvention == ViewNamingConvention.ControllerPrefix)
            {
                var viewname = controller.RouteData.Values["controller"].ToString() + controller.RouteData.Values["action"].ToString();
                this.OnSuccess((response) => _controller.View(viewname, response));
            }

            //default action for failure;
            _onFailure = _onSuccess;
        }

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
            _onSuccess = always;
            _onFailure = always;
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

            var responseValue = default(T);
            if (!_controller.ModelState.IsValid && !IgnoreModelState)
            {
                this.IgnoreModelState = true;
                return await _onFailure(responseValue);
            }
            var response = await _action();
            responseValue = response.Result;
            if (!response.Success)
            {
                response.AddToModelState(_controller);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new HttpNotFoundResult(response.Error);
                }
                this.IgnoreModelState = true;
                return await _onFailure(response.Result);
            }

            return await _onSuccess(response.Result);
        }
    }
}