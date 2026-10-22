using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class ReferenceDataMutationProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public ReferenceDataMutationProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new ReferenceDataMutationProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new ReferenceDataMutationProvider(_mocktokenProvider.Object, null));
        }


        [Fact]
        public async Task GetSegmentsDistance_ShouldSuccess()
        {
            //Arrange
            var mockRequest = TestDataProvider.GetTestData<SkyPartnerMSRequest>("Loyalty//SkyPartnerMSRequest");
            var mockResponse = "4523";

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("XYZ");  // Dummy token

            _mockConnector.Setup(m => m.GetAsync<string>(It.IsAny<string>(),
                                                                    It.IsAny<Dictionary<string, string>>(),
                                                                    It.IsAny<string>())).Returns(Task.FromResult(new APIResponse<string> { Response = mockResponse }));

            //Act
            var provider = new ReferenceDataMutationProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var result = await provider.GetSegmentsDistance(mockRequest.ticketInfo.FlightSegmentOrigin, mockRequest.ticketInfo.FlightSegmentDestination);

            //Assert           
            Assert.NotNull(result);
            Assert.Equal(mockResponse, result);
        }

        [Fact]
        public async Task GetBookingDetail_ShouldSuccess()
        {
            //Arrange
            var mockRequest = TestDataProvider.GetTestData<CabinClassDetailsRequest>("Loyalty//BookingDetailRequest");
            var mockResponse = TestDataProvider.GetTestData<CabinClassDetailsResponse>("Loyalty//BookingDetailResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("XYZ");  // Dummy token           
            _mockConnector.Setup(m => m.SendAsync<CabinClassDetailsResponse>(It.IsAny<string>(),
                                                       It.IsAny<Dictionary<string, string>>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<HttpMethod>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<CabinClassDetailsResponse> { Response = mockResponse }));

            //Act
            var provider = new ReferenceDataMutationProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var result = await provider.GetCabinClassDetails(mockRequest);

            //Assert           
            Assert.NotNull(result);
            Assert.Equal(mockResponse.CabinClass, result.CabinClass);
            Assert.Equal(mockResponse.OperatingBookingClass, result.OperatingBookingClass);
        }

    }
}
