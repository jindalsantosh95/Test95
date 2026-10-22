using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest;
using VAA.EAPI.GraphQL.Domain.Operations.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Operations.Tests
{
    public class PassengerManifestProviderTests
    {
        private readonly Mock<ILogger<PassengerManifestProvider_SCVv7>> _logger;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly Mock<IHttpConnector> _msConnector;

        public PassengerManifestProviderTests()
        {
            _logger = new Mock<ILogger<PassengerManifestProvider_SCVv7>>();
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _msConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowException_WithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new PassengerManifestProvider_SCVv7(null, _msConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new PassengerManifestProvider_SCVv7(_tokenProvider.Object, null));
        }

        [Fact]
        public async Task Query_ShouldSucceed()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<PassengerManifestRequest>("Operations\\PaxManifestReq");
            var providerResponse = TestDataProvider.GetTestData<APIResponse<DTOs.PassengerManifest_SCVv7.SCVv7_API_Response>>("Operations\\PaxManifestAPIResponse");
            _tokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            _msConnector.Setup(m => m.GetAsync<DTOs.PassengerManifest_SCVv7.SCVv7_API_Response>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(providerResponse);

            var provider = new PassengerManifestProvider_SCVv7(_tokenProvider.Object, _msConnector.Object);

            // Act
            var queryResponse = await provider.Query(request);

            // Assert
            Assert.IsType<PassengerManifestResponse>(queryResponse);
            Assert.Equal(queryResponse.Passengers.Count, providerResponse.Response.FlightDetails.CustomerDetails.Length);

        }
    }
}
