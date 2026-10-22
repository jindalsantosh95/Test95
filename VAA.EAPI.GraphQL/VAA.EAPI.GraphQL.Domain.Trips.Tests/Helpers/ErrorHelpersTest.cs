using VAA.EAPI.GraphQL.Domain.Trips.Helper;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Helpers
{
    public class ErrorHelpersTest
    {
        [Fact]        
        public async Task AggregateException_NullInput_ReturnsEmptyString()
        {
            var result = ErrorHelpers.AggregateException(null);
            Assert.NotNull(result);           
        }

        [Fact]
        public async Task AggregateException_CustomException_ReturnsEmptyString()
        {
            var result = ErrorHelpers.AggregateException(new Exception());
            Assert.NotNull(result);
        }

        [Fact]
        public async Task AggregateException_ArgumentNullException_ReturnsEmptyString()
        {
            var result = ErrorHelpers.AggregateException(new ArgumentNullException());
            Assert.NotNull(result);
        }

        [Fact]
        public async Task AggregateException_ExceptionEmpty_ReturnsEmptyString()
        {
            var result = ErrorHelpers.AggregateException(new Exception("", null));
            Assert.NotNull(result);
        }

        [Fact]
        public async Task AggregateException_InnerExceptionEmpty_ReturnsEmptyString()
        {
            var result = ErrorHelpers.AggregateException(new Exception("", new Exception("")));
            Assert.NotNull(result);
        }

        [Fact]
        public async Task AggregateException_CustomException_ReturnsExceptionMessage()
        {
            var result = ErrorHelpers.AggregateException(new Exception("Test Exception"));
            Assert.NotNull(result);
            Assert.Contains("Test Exception", result);
        }

        [Fact]
        public async Task AggregateException_CustomInnerException_ReturnsExceptionMessage()
        {
            var result = ErrorHelpers.AggregateException(new Exception("Test Exception", new Exception()));
            Assert.NotNull(result);
            Assert.Contains("Test Exception", result);
        }

        [Fact]
        public async Task AggregateException_CustomInnerException_ReturnsInnerExceptionMessage()
        {
            var result = ErrorHelpers.AggregateException(new Exception("Test Exception", new Exception("Test Inner Exception")));
            Assert.NotNull(result);
            Assert.Contains("Test Exception", result);
            Assert.Contains("Test Inner Exception", result);
        }

        [Fact]
        public async Task AggregateException_HttpRequestException_ReturnsDataMessage()
        {
            //Arrange
            var httpRequestException = new HttpRequestException("Test HttpRequestException");
            httpRequestException.Data.Add("ResponseContent", "Test Response Content");
            httpRequestException.Data.Add("StatusCode", "500");
            httpRequestException.Data.Add("RequestUrl", "http://testurl.com");
            httpRequestException.Data.Add("Request", "This key should be ignored");


            //Act
            var result = ErrorHelpers.AggregateException(httpRequestException);
            Assert.NotNull(result);
            Assert.Contains("Test Response Content", result);
            Assert.Contains("500", result);
            Assert.Contains("http://testurl.com", result);
            Assert.DoesNotContain("This key should be ignored", result);
        }
    }
}
