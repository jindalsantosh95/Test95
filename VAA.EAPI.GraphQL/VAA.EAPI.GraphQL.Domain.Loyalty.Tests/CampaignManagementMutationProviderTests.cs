using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CampaignManagementMutationProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;
        private readonly CampaignManagementMutationProvider _mutationProvider;

        public CampaignManagementMutationProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();
            _mutationProvider = new CampaignManagementMutationProvider(_mocktokenProvider.Object, _mockConnector.Object);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new CampaignManagementMutationProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new CampaignManagementMutationProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task UnassignCampaign_ArgumentNullException()
        {
            //Arrange
            CampaignUnassignMutationRequest request = null;

            //Act
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutationProvider.UnassignCampaign(request));
        }

        [Fact]
        public async Task UnassignCampaign_CustomerIdArgumentNullException()
        {
            //Arrange
            CampaignUnassignMutationRequest request = new CampaignUnassignMutationRequest()
            {
                CustomerId = null,
                DocumentId = "A",
            };

            //Act
            await Assert.ThrowsAsync<ArgumentException>(() => _mutationProvider.UnassignCampaign(request));
        }

        [Fact]
        public async Task UnassignCampaign_documentIdArgumentNullException()
        {
            //Arrange
            CampaignUnassignMutationRequest request = new CampaignUnassignMutationRequest()
            {
                CustomerId = "A",
                DocumentId = null
            };

            //Act
            await Assert.ThrowsAsync<ArgumentException>(() => _mutationProvider.UnassignCampaign(request));
        }

        [Fact]
        public async Task UnassignCampaign_ApiCallSuccess()
        {
            //Arrange
            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            CampaignUnassignMutationResponse resp = new CampaignUnassignMutationResponse()
            {
                Value = "Un-assign campaign completed successfully"
            };
            APIResponse<CampaignUnassignMutationResponse> response = new()
            {
                Response = resp
            };


            _mockConnector.Setup(m => m.SendAsync<CampaignUnassignMutationResponse>(It.IsAny<string>(),
                                                         It.IsAny<Dictionary<string, string>>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<HttpMethod>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<bool>())).Returns(Task.FromResult(response));



            CampaignUnassignMutationRequest request = new CampaignUnassignMutationRequest()
            {
                CustomerId = "A",
                DocumentId = "B"
            };

            //Act
            var result = await _mutationProvider.UnassignCampaign(request);

            //Assert
            Assert.Equal("Un-assign campaign completed successfully", result.Value);
        }

        [Fact]
        public async Task LinkCampaignPromotion_ApiCallSuccess()
        {
            //Arrange
            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            APIResponse<CampaignUpdateMutationResponse> response = new()
            {
                Response = new CampaignUpdateMutationResponse
                { Value = "Ok" }
            };
            _mockConnector.Setup(m => m.SendAsync<CampaignUpdateMutationResponse>(It.IsAny<string>(),
                                                         It.IsAny<Dictionary<string, string>>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<HttpMethod>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<bool>())).Returns(Task.FromResult(response));
            CampaignLinkingMutationRequest request = new CampaignLinkingMutationRequest()
            {
                CampaignId = "1-A976-1",
                PromotionId = "1-2OHIEB",
                CreatedBy = "test@email.com"
            };

            //Act
            var result = await _mutationProvider.LinkCampaignPromotion(request);

            //Assert
            Assert.Equal("Ok", result);
        }

        [Fact]
        public async Task LinkCampaignPromotion_ArgumentNullException()
        {
            //Arrange
            CampaignLinkingMutationRequest request = null;

            //Act
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutationProvider.LinkCampaignPromotion(request));
        }

        [Fact]
        public async Task DelinkCampaignPromotion_ApiCallSuccess()
        {
            //Arrange
            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            APIResponse<CampaignUpdateMutationResponse> response = new()
            {
                Response = new CampaignUpdateMutationResponse
                { Value = "Ok" }
            };
            _mockConnector.Setup(m => m.SendAsync<CampaignUpdateMutationResponse>(It.IsAny<string>(),
                                                         It.IsAny<Dictionary<string, string>>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<HttpMethod>(),
                                                         It.IsAny<string>(),
                                                         It.IsAny<bool>())).Returns(Task.FromResult(response));
            CampaignLinkingMutationRequest request = new CampaignLinkingMutationRequest()
            {
                CampaignId = "1-A976-1",
                PromotionId = "1-2OHIEB"
            };

            //Act
            var result = await _mutationProvider.DelinkCampaignPromotion(request);

            //Assert
            Assert.Equal("Ok", result);
        }

        [Fact]
        public async Task DelinkCampaignPromotion_ArgumentNullException()
        {
            //Arrange
            CampaignLinkingMutationRequest request = null;

            //Act
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutationProvider.DelinkCampaignPromotion(request));
        }
    }
}
