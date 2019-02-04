using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC.Tests
{
    public class MoqController
    {
        protected Mock<HttpContextBase> HttpContextBaseMock;
        protected Mock<HttpRequestBase> HttpRequestMock;
        protected Mock<HttpResponseBase> HttpResponseMock;

        public MoqController()
        {
            HttpContextBaseMock = new Mock<HttpContextBase>();
            HttpRequestMock = new Mock<HttpRequestBase>();
            HttpResponseMock = new Mock<HttpResponseBase>();
            HttpContextBaseMock.SetupGet(x => x.Request).Returns(HttpRequestMock.Object);
            HttpContextBaseMock.SetupGet(x => x.Response).Returns(HttpResponseMock.Object);

            var output = new StringWriter();
            HttpResponseMock.SetupGet(x => x.Output).Returns(output);
            HttpResponseMock.Setup(x => x.Write(It.IsAny<string>()))
                .Callback<string>(s => output.Write(s));
            HttpResponseMock.SetupProperty(x => x.ContentType);
        }

        protected T SetupController<T>() where T : Controller, new()
        {
            var routes = new RouteCollection();
            var controller = new T();
            controller.ControllerContext = new ControllerContext(HttpContextBaseMock.Object, new RouteData(), controller);
            controller.Url = new UrlHelper(new RequestContext(HttpContextBaseMock.Object, new RouteData()), routes);
            return controller;
        }

    }
}
