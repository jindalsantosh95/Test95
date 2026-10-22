using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.SFPD;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class LoyaltySyncMutationProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public LoyaltySyncMutationProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new LoyaltySyncMutationProvider<SFPDSyncResponse>(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new LoyaltySyncMutationProvider<SFPDSyncResponse>(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task SFPDSyncShouldSuccess()
        {
            //Arrange
            var mockmutationRequest = TestDataProvider.GetTestData<SFPDSyncMutationRequest>("Loyalty//SFPDSyncRequest");
            var mockRequest = (SFPDSyncRequest)mockmutationRequest;
            var mockResponse = TestDataProvider.GetTestData<SFPDSyncResponse>("Loyalty//SFPDSyncResponse");
            mockResponse.Errors = null;

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("XYZ");  // Dummy token           
            _mockConnector.Setup(m => m.SendAsync<SFPDSyncResponse>(It.IsAny<string>(),
                                                       It.IsAny<Dictionary<string, string>>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<HttpMethod>(),
                                                       It.IsAny<string>(),
                                                       It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<SFPDSyncResponse> { Response = mockResponse }));

            //Act
            var provider = new LoyaltySyncMutationProvider<SFPDSyncResponse>(_mocktokenProvider.Object, _mockConnector.Object);
            var result = await provider.Mutate(mockRequest);

            //Assert
            Assert.Equal(mockResponse.PsdsSyncStatus, result.PsdsSyncStatus);
            Assert.Null(result.Errors);
            Assert.Equal(mockResponse.CommandStatus, result.CommandStatus);
        }

    }
}
