using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class AddressDetailsQueryTests
    {
        private readonly Mock<ILogger<AddressDetailsQuery>> _mockLogger;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly Mock<IHttpConnector> _connector;
        private readonly AddressDetailsQuery _query;

        public AddressDetailsQueryTests()
        {
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _mockLogger = new Mock<ILogger<AddressDetailsQuery>>();
            _connector = new Mock<IHttpConnector>();
            _query = new AddressDetailsQuery(_tokenProvider.Object, _connector.Object, _mockLogger.Object);

            Setup.CreateTestEnvironmentSettings();
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddressDetailsQuery(_tokenProvider.Object, _connector.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhentokeProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddressDetailsQuery(null, _connector.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenDConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddressDetailsQuery(_tokenProvider.Object, null, _mockLogger.Object));
        }

        [Fact]
        public async Task Query_ShouldSucceed_WithGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressDetailRequest>("Account//AddressDetailsQueryRequest");
            var weselAddressResponse = TestDataProvider.GetTestData<APIResponse<List<WesalAddressResponse>>>("Account\\WeselAddressDetailsResponse");

            ConfigReader.Set(Common.Constants.Environment.EnableAzureEndpointAddressDetails, "false");

            _connector.Setup(c => c.SendAsync<List<WesalAddressResponse>>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(weselAddressResponse);

            //Act
            var result = await _query.FCAddressDetails(request);

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Query_ShouldReturnException_FailureFromAPI()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressDetailRequest>("Account//AddressDetailsQueryRequest");
            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity };

            ConfigReader.Set(Common.Constants.Environment.EnableAzureEndpointAddressDetails, "false");

            _connector.Setup(x => x.SendAsync<List<WesalAddressResponse>>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _query.FCAddressDetails(request));
        }

        [Fact]
        public async Task Query_ShouldThrowException_WithNullRequest()
        {
            var request = TestDataProvider.GetTestData<AddressDetailRequest>("Account//AddressDetailsQueryRequest");

            await Assert.ThrowsAsync<ArgumentNullException>(() => _query.FCAddressDetails(request = null));
        }
    }
}

