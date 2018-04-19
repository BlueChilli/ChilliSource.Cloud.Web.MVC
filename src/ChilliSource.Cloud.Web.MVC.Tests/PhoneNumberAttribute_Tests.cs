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
    public class PhoneNumberAttribute_Tests
    {
        [Fact]
        public void SimpleTest()
        {
            var att = new PhoneNumberAttribute("AU");

            Assert.True(att.IsValid("0411111111"));
        }
    }
}
