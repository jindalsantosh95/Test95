using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Tests
{
    public class GetBookingQueryTests
    {
        private readonly Mock<IHttpConnector> _mockConnector;
        private readonly GetBookingQuery _query;
        private readonly Mock<IOAuthTokenProvider> _authTokenProvider;
        private Mock<ILogger<GetBookingQuery>> _logger;

        public GetBookingQueryTests()
        {
            _mockConnector = new Mock<IHttpConnector>();
            _authTokenProvider = new Mock<IOAuthTokenProvider>();
            _logger = new Mock<ILogger<GetBookingQuery>>();
            _query = new GetBookingQuery(_logger.Object, _authTokenProvider.Object, _mockConnector.Object);

            Setup.CreateTestEnvironmentSettings();
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMockLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new GetBookingQuery(null, _authTokenProvider.Object, _mockConnector.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenAuthTokenProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new GetBookingQuery(_logger.Object, null, _mockConnector.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMockConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new GetBookingQuery(_logger.Object, _authTokenProvider.Object, null));
        }
        [Fact]
        public async Task GetBookingQuery_ShouldReturnSucceed_WithGoodResponse()
        {
            var response = TestDataProvider.GetTestData<GetBookingResponse>("Servicing\\GetBookingResponseData");
            var apiReponse = new APIResponse<GetBookingResponse>()
            {
                Response = response
            };
            var fcNumber = "Test fcNumber";
            var phoneNo = "Test phoneNo";
            _mockConnector.Setup(x =>
            x.SendAsync<GetBookingResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(apiReponse);

            var result = await _query.Getbooking(fcNumber, phoneNo);

            Assert.NotNull(result);
        }
        [Fact]
        public async Task GetBookingQuery_ShouldThrowNullException_WithBadRequest()
        {
            var fcNumber = "Test fcNumber";
            var phoneNo = "Test phoneNo";
            await Assert.ThrowsAsync<NullReferenceException>(() => _query.Getbooking(fcNumber, phoneNo));
        }
    }
}
