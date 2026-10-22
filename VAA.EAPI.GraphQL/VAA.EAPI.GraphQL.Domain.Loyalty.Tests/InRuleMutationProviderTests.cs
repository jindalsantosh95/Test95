using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class InRuleMutationProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public InRuleMutationProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new InRuleMutationProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new InRuleMutationProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task CalculatePoints_ShouldSuccess()
        {
            //Arrange
            var mockRequest = TestDataProvider.GetTestData<AirlineEarnedPointsRequest>("Loyalty//AirlineEarnedPointsRequest");
            var mockResponse = TestDataProvider.GetTestData<CreditPointsResponse>("Loyalty//CreditPointsResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("XYZ");  // Dummy token           
            _mockConnector.Setup(m => m.SendAsync<CreditPointsResponse>(It.IsAny<string>(),
                                                       It.IsAny<Dictionary<string, string>>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<HttpMethod>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<CreditPointsResponse> { Response = mockResponse }));

            //Act
            var provider = new InRuleMutationProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var result = await provider.CalculatePoints(mockRequest);

            //Assert            
            Assert.Null(result.Errors);
            Assert.NotNull(result.SegmentResponses);
            Assert.Equal(mockResponse.SegmentResponses.First().AirlineCodeOperating, result.SegmentResponses.First().AirlineCodeOperating);
            Assert.Equal(mockResponse.SegmentResponses.First().TierBonusPoints, result.SegmentResponses.First().TierBonusPoints);
            Assert.Equal(mockResponse.SegmentResponses.First().BasePoints, result.SegmentResponses.First().BasePoints);
            Assert.Equal(mockResponse.SegmentResponses.First().SegmentClass, result.SegmentResponses.First().SegmentClass);
            Assert.Equal(mockResponse.SegmentResponses.First().Destination, result.SegmentResponses.First().Destination);
            Assert.Equal(mockResponse.SegmentResponses.First().Origin, result.SegmentResponses.First().Origin);
            Assert.Equal(mockResponse.SegmentResponses.First().FlightDate, result.SegmentResponses.First().FlightDate);
            Assert.Equal(mockResponse.SegmentResponses.First().FlightNumberOperating, result.SegmentResponses.First().FlightNumberOperating);
            Assert.Equal(mockResponse.SegmentResponses.First().TierPointsAward, result.SegmentResponses.First().TierPointsAward);
            Assert.Equal(mockResponse.SegmentResponses.First().CabinBonusPoints, result.SegmentResponses.First().CabinBonusPoints);
            Assert.Equal(mockResponse.SegmentResponses.First().AirlineCodeOperating, result.SegmentResponses.First().AirlineCodeOperating);
        }

    }
}
