using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class SkyPartnerRetroMutationProviderTest
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public SkyPartnerRetroMutationProviderTest()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new SkyPartnerRetroMutationProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new SkyPartnerRetroMutationProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task GetFlyingActivityDetail_ShouldSuccess()
        {
            //Arrange
            var mockRequest = TestDataProvider.GetTestData<SkyPartnerMSRequest>("Loyalty//SkyPartnerMSRequest");
            var mockResponse = TestDataProvider.GetTestData<FlyingActivityDetails>("Loyalty//FlyingActivityDetail");
            mockResponse.Errors = null;

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("XYZ");  // Dummy token           
            _mockConnector.Setup(m => m.SendAsync<FlyingActivityDetails>(It.IsAny<string>(),
                                                       It.IsAny<Dictionary<string, string>>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<HttpMethod>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<FlyingActivityDetails> { Response = mockResponse }));

            //Act
            var provider = new SkyPartnerRetroMutationProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var result = await provider.GetFlyingActivityDetails(mockRequest);

            //Assert            
            Assert.Null(result.Errors);
            Assert.Equal(mockResponse.TransactionId, result.TransactionId);
            Assert.Equal(mockResponse.IsEligible, result.IsEligible);
            Assert.Equal(mockResponse.EligibilityDenialCode, result.EligibilityDenialCode);
            Assert.Equal(mockResponse.SkyClaimResponse.EticketNumber, result.SkyClaimResponse.EticketNumber);
            Assert.Equal(mockResponse.SkyClaimResponse.FfpAccountNumber, result.SkyClaimResponse.FfpAccountNumber);
            Assert.Equal(mockResponse.SkyClaimResponse.CabinClassCode, result.SkyClaimResponse.CabinClassCode);
            Assert.Equal(mockResponse.SkyClaimResponse.MarketingFlightNumber, result.SkyClaimResponse.MarketingFlightNumber);
            Assert.Equal(mockResponse.SkyClaimResponse.OperatingFlightNumber, result.SkyClaimResponse.OperatingFlightNumber);
            Assert.Equal(mockResponse.SkyClaimResponse.FfpCarrierCode, result.SkyClaimResponse.FfpCarrierCode);
            Assert.Equal(mockResponse.SkyClaimResponse.PassengerFirstName, result.SkyClaimResponse.PassengerFirstName);
        }

        [Fact]
        public async Task GetFlyingActivityDetail_WithInvalidFlightDate()
        {
            //Arrange
            var mockRequest = TestDataProvider.GetTestData<SkyPartnerMSRequest>("Loyalty//SkyPartnerMSRequest");
            mockRequest.ticketInfo.ScheduledFlightSegmentDepartureDate = "2025-11-01";

            var mockResponse = TestDataProvider.GetTestData<FlyingActivityDetails>("Loyalty//FlyingActivityDetail");
            mockResponse.SkyClaimResponse = null;

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("XYZ");  // Dummy token           
            _mockConnector.Setup(m => m.SendAsync<FlyingActivityDetails>(It.IsAny<string>(),
                                                       It.IsAny<Dictionary<string, string>>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<HttpMethod>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<FlyingActivityDetails> { Response = mockResponse }));

            //Act
            var provider = new SkyPartnerRetroMutationProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var result = await provider.GetFlyingActivityDetails(mockRequest);

            //Assert            
            Assert.NotNull(result.Errors);
            Assert.Null(result.SkyClaimResponse);
            Assert.Equal(mockResponse.Errors.First().Message, result.Errors.First().Message);
            Assert.Equal(mockResponse.Errors.First().Code, result.Errors.First().Code);
            Assert.Equal(mockResponse.EligibilityDenialCode, result.EligibilityDenialCode);
        }

        [Fact]
        public async Task GetFlyingActivityDetail_WithInvalidOrginAndDestination()
        {
            //Arrange
            var mockRequest = TestDataProvider.GetTestData<SkyPartnerMSRequest>("Loyalty//SkyPartnerMSRequest");
            mockRequest.ticketInfo.FlightSegmentOrigin = "MAS";
            mockRequest.ticketInfo.FlightSegmentDestination = "YVR";

            var mockResponse = TestDataProvider.GetTestData<FlyingActivityDetails>("Loyalty//FlyingActivityDetail");
            mockResponse.SkyClaimResponse = null;
            mockResponse.Errors.First().Message = "The origin/ destination entered does not match with the flight number";

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("XYZ");  // Dummy token           
            _mockConnector.Setup(m => m.SendAsync<FlyingActivityDetails>(It.IsAny<string>(),
                                                       It.IsAny<Dictionary<string, string>>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<HttpMethod>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<FlyingActivityDetails> { Response = mockResponse }));

            //Act
            var provider = new SkyPartnerRetroMutationProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var result = await provider.GetFlyingActivityDetails(mockRequest);

            //Assert            
            Assert.NotNull(result.Errors);
            Assert.Null(result.SkyClaimResponse);
            Assert.Equal(mockResponse.Errors.First().Message, result.Errors.First().Message);
            Assert.Equal(mockResponse.Errors.First().Code, result.Errors.First().Code);
            Assert.Equal(mockResponse.EligibilityDenialCode, result.EligibilityDenialCode);
        }

        [Fact]
        public async Task RetroCreditFeedback_ShouldSuccess()
        {
            //Arrange
            var mockRequest = TestDataProvider.GetTestData<SkyMSFeedbackRequest>("Loyalty//SkyMSFeedbackRequest");
            var mockResponse = TestDataProvider.GetTestData<SkyMSSuccessResponse>("Loyalty//SkyMSSuccessResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("XYZ");  // Dummy token           
            _mockConnector.Setup(m => m.SendAsync<SkyMSSuccessResponse>(It.IsAny<string>(),
                                                       It.IsAny<Dictionary<string, string>>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<HttpMethod>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<SkyMSSuccessResponse> { Response = mockResponse }));

            //Act
            var provider = new SkyPartnerRetroMutationProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var result = await provider.RetroCreditFeedback(mockRequest);

            //Assert            
            Assert.Null(result.Errors);
            Assert.Equal(mockResponse.TrackID, result.TrackID);
            Assert.Equal(mockResponse.TransactionId, result.TransactionId);
            Assert.Equal(mockResponse.EligibilityDenialCode, result.EligibilityDenialCode);
            Assert.Equal(mockResponse.UniqueIdentificationNumber, result.UniqueIdentificationNumber);
            Assert.Equal(mockResponse.CustomerId, result.CustomerId);
        }
    }
}
