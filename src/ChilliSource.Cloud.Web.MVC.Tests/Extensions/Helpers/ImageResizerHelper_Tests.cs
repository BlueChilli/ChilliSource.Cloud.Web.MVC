using ChilliSource.Cloud.Core;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Xunit;

namespace ChilliSource.Cloud.Web.MVC.Tests
{
    public class ImageResizerHelper_Tests
    {
        public ImageResizerHelper_Tests()
        {
            GlobalWebConfiguration.Instance.BaseUrl = "https://www.mysite.com";
        }

        [Fact]
        public void ImageUrl_ReturnsCorrectPath()
        {
            var helper = new ImageResizerHelper("~/storage/default");

            var url = helper.ImageUrl("~/Images/logo.png");
            Assert.Equal("/Images/logo.png", url);

            url = helper.ImageUrl("Assets/123456.jpg?hello=monkey");
            Assert.Equal("/storage/default/Assets/123456.jpg", url);

            url = helper.ImageUrl("Assets/123456.jpg", new ImageResizerCommand { Width = 100 });
            Assert.Equal("/storage/default/Assets/123456.jpg?w=100&autorotate=true", url);

            url = helper.ImageUrl("", new ImageResizerCommand(), "~/Images/generic-logo.png");
            Assert.Equal("/Images/generic-logo.png?autorotate=true", url);

            url = helper.ImageUrl("~/Images/logo.png", fullPath: true);
            Assert.Equal("https://www.mysite.com/Images/logo.png", url);

            url = helper.ImageUrl("Images/logo.png", fullPath: true);
            Assert.Equal("https://www.mysite.com/storage/default/Images/logo.png", url);

        }

        [Fact]
        public void Image_ReturnsImageHtml()
        {
            var helper = new ImageResizerHelper("~/storage/default");

            var image = helper.Image("Assets/123456.jpg", 200, 200);
            Assert.Equal("<img height=\"200\" src=\"/storage/default/Assets/123456.jpg?h=200&amp;w=200&amp;autorotate=true\" width=\"200\" />", image.ToHtmlString());
        }
    }
}
