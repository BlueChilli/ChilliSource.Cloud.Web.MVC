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
