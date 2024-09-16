namespace ChilliSource.Cloud.Web.MVC.Tests
{
    public class ConvertAttemptedValueToBoolean_Tests
    {
        [Fact]
        public void SimpleTest()
        {
            Assert.True(HtmlHelperExtensions.ConvertAttemptedValueToBoolean(true));
            Assert.True(HtmlHelperExtensions.ConvertAttemptedValueToBoolean("true"));
            Assert.False(HtmlHelperExtensions.ConvertAttemptedValueToBoolean("false,true"));

            string? s = null;
            Assert.False(HtmlHelperExtensions.ConvertAttemptedValueToBoolean(s));
            Assert.False(HtmlHelperExtensions.ConvertAttemptedValueToBoolean("truerun type %SYSTEMROOT%\\win.ini"));
        }
    }
}
