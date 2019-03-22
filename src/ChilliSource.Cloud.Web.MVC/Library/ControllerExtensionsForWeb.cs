#if !NET_4X
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Web.MVC
{
    public static class ControllerExtensionsForWeb
    {
        [DebuggerNonUserCode]
        public static IServiceCallerSyntax<T> ServiceCall<T>(this Controller controller, Func<ServiceResult<T>> action)
        {
            return (new ServiceCaller<T>(controller)).SetAction(action);
        }

        [DebuggerNonUserCode]
        public static IServiceCallerSyntax<PagedList<T>> ServiceCall<T>(this Controller controller, Func<PagedList<T>> action)
        {
            Func<ServiceResult<PagedList<T>>> wrappedAction = () => ServiceResult<PagedList<T>>.AsSuccess(action());

            return (new ServiceCaller<PagedList<T>>(controller)).SetAction(wrappedAction);
        }

        [DebuggerNonUserCode]
        public static IServiceCallerAsyncSyntax<T> ServiceCall<T>(this Controller controller, Func<Task<ServiceResult<T>>> action)
        {
            return (new ServiceCallerAsync<T>(controller)).SetAction(action);
        }

        [DebuggerNonUserCode]
        public static IServiceCallerAsyncSyntax<PagedList<T>> ServiceCall<T>(this Controller controller, Func<Task<PagedList<T>>> action)
        {
            Func<Task<ServiceResult<PagedList<T>>>> wrappedAction = async () => ServiceResult<PagedList<T>>.AsSuccess(await action());

            return (new ServiceCallerAsync<PagedList<T>>(controller)).SetAction(wrappedAction);
        }
    }
}
#endif