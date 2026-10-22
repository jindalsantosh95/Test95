using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Operations.Interfaces;
using VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest;
using VAA.EAPI.GraphQL.Domain.Operations.Queries;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Operations.Tests
{
    public class PassengerManifestQueryTests
    {
        private readonly Mock<ILogger<PassengerManifestQuery>> _logger;
        private readonly Mock<IPassengerManifestProvider> _provider;

        public PassengerManifestQueryTests()
        {
            _logger = new Mock<ILogger<PassengerManifestQuery>>();
            _provider = new Mock<IPassengerManifestProvider>();
        }

        [Fact]
        public void Constructor_ShouldThrowException_WithNullParameters()
        {
            Assert.Throws<ArgumentNullException>(() => new PassengerManifestQuery(null, null));
            Assert.Throws<ArgumentNullException>(() => new PassengerManifestQuery(_provider.Object, null));
        }

        [Fact]
        public async Task PassengerManifest_ShouldThrowException_WithNullParameters()
        {

            // Arrange
            var query = new PassengerManifestQuery(_provider.Object, _logger.Object);
            _provider.Setup(m => m.Query(It.IsAny<PassengerManifestRequest>())).ReturnsAsync(new PassengerManifestResponse());

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => query.PassengerManifest(null));
        }

        [Fact]
        public async Task PassengerManifest_ShouldSucceed()
        {
            // Arrange
            var responseData = TestDataProvider.GetTestData<PassengerManifestResponse>("Operations\\PaxManifestResp");
            var query = new PassengerManifestQuery(_provider.Object, _logger.Object);
            _provider.Setup(m => m.Query(It.IsAny<PassengerManifestRequest>())).ReturnsAsync(responseData);
            var request = TestDataProvider.GetTestData<PassengerManifestRequest>("Operations\\PaxManifestReq");


            // Act
            var queryResponse = await query.PassengerManifest(request);

            // Assert
            Assert.IsType<PassengerManifestResponse>(queryResponse);
            //Assert.True(responseData.FlightDetails.CustomerDetails.Length == queryResponse.FlightDetails.CustomerDetails.Length);
        }

        [Fact]
        public async Task PassengerManifest_ShouldThrowException_APIFailure()
        {
            // Arrange
            var query = new PassengerManifestQuery(_provider.Object, _logger.Object);
            _provider.Setup(m => m.Query(It.IsAny<PassengerManifestRequest>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(System.Net.HttpStatusCode.InternalServerError), System.Net.HttpStatusCode.InternalServerError));

            // Act
            await Assert.ThrowsAsync<HttpRequestException>(() => query.PassengerManifest(new()));
        }

        [Fact]
        public async Task PassengerManifest_ShouldThrowException_GeneralException()
        {
            // Arrange
            var query = new PassengerManifestQuery(_provider.Object, _logger.Object);
            _provider.Setup(m => m.Query(It.IsAny<PassengerManifestRequest>())).ThrowsAsync(new Exception());

            // Act
            await Assert.ThrowsAsync<Exception>(() => query.PassengerManifest(new()));
        }
    }
}