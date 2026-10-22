using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper;
using Xunit;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Tests
{
    public class ErrorHelperTests
    {
        [Theory]
        [InlineData(HttpStatusCode.NotFound, "HTTP_404")]
        [InlineData(HttpStatusCode.ServiceUnavailable, "HTTP_503")]
        public void TravelRequirementsErrorMapper_UsesHttpStatusCode_WhenResponseHasNoErrorCode(HttpStatusCode statusCode, string expectedCode)
        {
            var exception = new HttpRequestException("Downstream request failed.", null, statusCode);

            var error = ErrorHelper.TravelRequirementsErrorMapper(exception, NullLogger.Instance);

            Assert.Equal(expectedCode, error.Code);
            Assert.Equal("Something went a little off course", error.Title);
        }

        [Fact]
        public void TravelRequirementsErrorMapper_PreservesTrmCode_WhenResponseContainsOne()
        {
            var exception = new HttpRequestException("Downstream request failed.", null, HttpStatusCode.BadRequest);
            exception.Data["Response"] = "{\"exceptions\":[{\"code\":\"travelrequirement_011_001\"}]}";

            var error = ErrorHelper.TravelRequirementsErrorMapper(exception, NullLogger.Instance);

            Assert.Equal("travelrequirement_011_001", error.Code);
        }

        [Fact]
        public void TravelRequirementsErrorMapper_PreservesTopLevelCode_WhenResponseContainsOne()
        {
            var exception = new HttpRequestException("Downstream request failed.", null, HttpStatusCode.NotFound);
            exception.Data["Response"] = "{\"code\":\"THP4004\"}";

            var error = ErrorHelper.TravelRequirementsErrorMapper(exception, NullLogger.Instance);

            Assert.Equal("THP4004", error.Code);
        }

        [Fact]
        public void TravelRequirementsErrorMapper_PreservesTopLevelCode_WhenExceptionsHasUnexpectedShape()
        {
            var exception = new HttpRequestException("Downstream request failed.", null, HttpStatusCode.NotFound);
            exception.Data["Response"] = "{\"exceptions\":\"unexpected\",\"code\":\"THP4004\"}";

            var error = ErrorHelper.TravelRequirementsErrorMapper(exception, NullLogger.Instance);

            Assert.Equal("THP4004", error.Code);
        }

        [Fact]
        public void TravelRequirementsErrorMapper_PreservesTopLevelCode_WhenEarlierCodesAreWhitespace()
        {
            var exception = new HttpRequestException("Downstream request failed.", null, HttpStatusCode.NotFound);
            exception.Data["Response"] = "{\"exceptions\":[{\"code\":\" \"}],\"batchResults\":[{\"exceptions\":[{\"code\":\"\"}]}],\"code\":\"THP4004\"}";

            var error = ErrorHelper.TravelRequirementsErrorMapper(exception, NullLogger.Instance);

            Assert.Equal("THP4004", error.Code);
        }

        [Fact]
        public void TravelRequirementsErrorMapper_PreservesTrmCode_WhenOtherResponseFieldsCannotBeDeserialized()
        {
            var exception = new HttpRequestException("Downstream request failed.", null, HttpStatusCode.BadRequest);
            exception.Data["Response"] = "{\"exceptions\":[{\"code\":\"travelrequirement_011_001\"}],\"batchResults\":\"unexpected\"}";

            var error = ErrorHelper.TravelRequirementsErrorMapper(exception, NullLogger.Instance);

            Assert.Equal("travelrequirement_011_001", error.Code);
        }

        [Fact]
        public void TravelRequirementsErrorMapper_UsesHttpStatusCode_WhenResponseIsMalformed()
        {
            var exception = new HttpRequestException("Downstream request failed.", null, HttpStatusCode.BadGateway);
            exception.Data["Response"] = "not json";

            var error = ErrorHelper.TravelRequirementsErrorMapper(exception, NullLogger.Instance);

            Assert.Equal("HTTP_502", error.Code);
        }

        [Fact]
        public void TravelRequirementsErrorMapper_LogsWarning_WhenResponseJsonIsMalformed()
        {
            const string responseJson = "not json";
            var exception = new HttpRequestException("Downstream request failed.", null, HttpStatusCode.BadRequest);
            exception.Data["Response"] = responseJson;
            var logger = new RecordingLogger();

            var error = ErrorHelper.TravelRequirementsErrorMapper(exception, logger);

            Assert.Equal("HTTP_400", error.Code);
            var log = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Warning, log.Level);
            Assert.Equal("Travel requirements error response JSON deserialization failed.", log.Message);
            Assert.Null(log.Exception);
        }

        [Fact]
        public void TravelRequirementsErrorMapper_UsesFirstBatchException_WithoutLoggingParseFailure()
        {
            var exception = new HttpRequestException("Downstream request failed.", null, HttpStatusCode.BadRequest);
            exception.Data["Response"] = "{\"batchResults\":[{\"exceptions\":[{\"code\":\"travelrequirement_030_005\"},{\"code\":\"travelrequirement_050_004\"}]}]}";
            var logger = new RecordingLogger();

            var error = ErrorHelper.TravelRequirementsErrorMapper(exception, logger);

            Assert.Equal("travelrequirement_030_005", error.Code);
            Assert.Empty(logger.Entries);
        }

        private sealed class RecordingLogger : ILogger
        {
            public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = [];

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NoopScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Entries.Add((logLevel, formatter(state, exception), exception));
            }
        }

        private sealed class NoopScope : IDisposable
        {
            public static readonly NoopScope Instance = new();

            public void Dispose() { }
        }
    }
}
