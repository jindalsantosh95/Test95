using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class HouseHoldMutationProviderTest
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;
        private readonly HouseHoldMutationProvider _mutationProvider;

        public HouseHoldMutationProviderTest()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();
            _mutationProvider = new HouseHoldMutationProvider(_mocktokenProvider.Object, _mockConnector.Object);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new HouseHoldMutationProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new HouseHoldMutationProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task HouseholdDelinkingShouldSuccess()
        {
            //Arrange
            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            _mockConnector.Setup(m => m.SendAsync<ResponseMessage>(It.IsAny<string>(),
                                                         It.IsAny<Dictionary<string, string>>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<HttpMethod>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<ResponseMessage> { RawResponse = "{\"message\":\"Request successfully initiated\"}" }));

            var apiRequest = TestDataProvider.GetTestData<HouseHoldDelinkingRequest>("Loyalty//HouseholdLinkDelinkAPIRequest");

            //Act
            var result = await _mutationProvider.HouseHoldDelinking(apiRequest);

            //Assert
            Assert.Contains("Request successfully initiated", result);
        }

        [Fact]
        public async Task HouseholdLinkingShouldSuccess()
        {
            //Arrange
            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            _mockConnector.Setup(m => m.SendAsync<ResponseMessage>(It.IsAny<string>(),
                                                         It.IsAny<Dictionary<string, string>>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<HttpMethod>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<bool>())).Returns(Task.FromResult(new APIResponse<ResponseMessage> { RawResponse = "{\"message\":\"Request successfully initiated\"}" }));

            var apiRequest = TestDataProvider.GetTestData<HouseHoldLinkingRequest>("Loyalty//HouseholdLinkDelinkAPIRequest");

            //Act
            var result = await _mutationProvider.HouseHoldLinking(apiRequest);

            //Assert
            Assert.Contains("Request successfully initiated", result);
        }
    }
}
