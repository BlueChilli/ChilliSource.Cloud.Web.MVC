using ChilliSource.Cloud.Core;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Xunit;

namespace ChilliSource.Cloud.Web.MVC.Tests
{
    public class JsonNetResult_Tests : MoqController
    {
        public JsonNetResult_Tests() 
        {
        }

        [Fact]
        public void Action_ReturnsCorrectJsonForOjbect()
        {
            var controller = SetupController<JsonNetResultController>();
            var result = controller.Action1();
            result.ExecuteResult(controller.ControllerContext);
            string content = controller.Response.Output.ToString();
            Assert.Equal("application/json", controller.Response.ContentType);
            Assert.Equal("{\"Name\":\"Jim\",\"Favourite\":{\"Method\":\"POST\"}}", content);
        }

    }

    public class JsonNetResultController : Controller
    {
        protected override JsonResult Json(
                object data,
                string contentType,
                System.Text.Encoding contentEncoding,
                JsonRequestBehavior behavior)
        {
            var json = new JsonNetResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
            return json;
        }

        public JsonResult Action1()
        {
            return Json(new { Name = "Jim", Favourite = System.Net.Http.HttpMethod.Post }, JsonRequestBehavior.AllowGet);
        }
    }
}
