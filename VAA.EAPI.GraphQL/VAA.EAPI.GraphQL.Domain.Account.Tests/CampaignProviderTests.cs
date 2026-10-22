using Microsoft.Extensions.DependencyInjection;
using Moq;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Models.Campaign;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;


namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class CampaignProviderTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public CampaignProviderTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            ConfigReader.Set("CampaignManagement:CampaignManagementMsUrl", "https://Testresources.virginatlantic.com/sit/campaignmanagement/");
            ConfigReader.Set("CampaignManagement:AppId", "abc");
            ConfigReader.Set("CampaignManagement:OcpSubID", "xyz");

            var _tokenProvider = new Mock<IOAuthTokenProvider>();
            _tokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token

            _serviceProvider = new ServiceCollection().AddLogging()
                                          .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                          .AddSingleton<IHttpConnector, HttpConnector>()
                                          .AddSingleton<IOAuthTokenProvider>(_tokenProvider.Object)
                                          .AddSingleton<CampaignProvider>()
                                          .BuildServiceProvider();

        }

        [Fact]
        public void Constructor_Throws_WhenTokenProviderIsNull()
        {
            var connectorMock = new Mock<IHttpConnector>();
            Assert.Throws<ArgumentNullException>(() => new CampaignProvider(null, connectorMock.Object));
        }

        [Fact]
        public void Constructor_Throws_WhenConnectorIsNull()
        {
            var tokenProviderMock = new Mock<IOAuthTokenProvider>();
            Assert.Throws<ArgumentNullException>(() => new CampaignProvider(tokenProviderMock.Object, null));
        }

        [Fact]
        public async Task ValidateCampaign_ReturnsExpectedResponse_WhenConnectorSucceeds()
        {
            var validateEligibilityQueries = _serviceProvider.GetService<CampaignProvider>()!;

            // Arrange
            var uniqueId = "UID-AVAILABLE";
            var expected = new ValidateEligibilityResponse
            {
                Valid = true,
                Claimed = true,
                FcNumber = "12089808",
                ClaimedDateTime = "2026-04-07T09:29:19.166Z"
            };

            var result = await validateEligibilityQueries.ValidateCampaign(uniqueId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Valid, result.Valid);
            Assert.Equal(expected.Claimed, result.Claimed);
            Assert.Equal(expected.FcNumber, result.FcNumber);
            Assert.Equal(expected.ClaimedDateTime, result.ClaimedDateTime);
        }

        [Fact]
        public async Task AssignCampaign_ReturnsExpectedResponse_WhenConnectorSucceeds()
        {

            var assignCampaignProvider = _serviceProvider.GetService<CampaignProvider>()!;

            // Arrange
            var request = new AssignCampaignRequest()
            {
                CampaignId = "12323",
                CreatedBy = "TestUser",
            };

            var result = await assignCampaignProvider.AssignCampaign(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Campaign assign initiated successfully", result);

        }

        [Fact]
        public async Task UpdateCampaign_ReturnsExpectedResponse_WhenConnectorSucceeds()
        {
            var updateCampaignProvider = _serviceProvider.GetService<CampaignProvider>()!;

            // Arrange
            var request = new UpdateCampaignRequest()
            {
                AmexUniqueId = "12323",
                FcNumber = "987789987",
            };

            var result = await updateCampaignProvider.UpdateCampaign(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Campagin Amex Update Successfully", result);
        }

        [Fact]
        public async Task ValidateCampaign_Throws_WhenConnectorThrows()
        {
            var validateEligibilityQueries = _serviceProvider.GetService<CampaignProvider>()!;

            // Arrange
            var uniqueId = "uid-bad";

            ConfigReader.Set("CampaignManagement:CampaignManagementMsUrl", "https://Testresources.virginatlantic.com/sit/campaignmanagement/bad/");

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => validateEligibilityQueries.ValidateCampaign(uniqueId));
        }

        [Fact]
        public async Task AssignCampaign_Throws_WhenConnectorThrows()
        {
            var assignCampaignProvider = _serviceProvider.GetService<CampaignProvider>()!;

            // Arrange
            var request = new AssignCampaignRequest()
            {
                CampaignId = "12323",
                CreatedBy = "TestUser",
            };
            ConfigReader.Set("CampaignManagement:CampaignManagementMsUrl", "https://Testresources.virginatlantic.com/sit/campaignmanagement/bad/");

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => assignCampaignProvider.AssignCampaign(request));
        }

        [Fact]
        public async Task UpdateCampaign_Throws_WhenConnectorThrows()
        {
            var updateCampaignProvider = _serviceProvider.GetService<CampaignProvider>()!;

            // Arrange
            var request = new UpdateCampaignRequest()
            {
                AmexUniqueId = "12323",
                FcNumber = "987789987",
            };
            ConfigReader.Set("CampaignManagement:CampaignManagementMsUrl", "https://Testresources.virginatlantic.com/sit/campaignmanagement/bad/");

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => updateCampaignProvider.UpdateCampaign(request));
        }
    }
}
