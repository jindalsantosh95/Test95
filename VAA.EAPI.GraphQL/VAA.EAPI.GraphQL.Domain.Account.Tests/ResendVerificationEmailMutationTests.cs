using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Framework.Tests;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class ResendVerificationEmailMutationTests
    {
        private readonly Mock<ILogger<ResendVerificationEmailMutation>> _mockLogger;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly Mock<IHttpConnector> _connector;
        private readonly ResendVerificationEmailMutation _mutation;

        public ResendVerificationEmailMutationTests()
        {
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _mockLogger = new Mock<ILogger<ResendVerificationEmailMutation>>();
            _connector = new Mock<IHttpConnector>();
            _mutation = new ResendVerificationEmailMutation(_mockLogger.Object, _tokenProvider.Object, _connector.Object);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ResendVerificationEmailMutation(null, _tokenProvider.Object, _connector.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhentokeProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ResendVerificationEmailMutation(_mockLogger.Object, null, _connector.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenDConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ResendVerificationEmailMutation(_mockLogger.Object, _tokenProvider.Object, null));
        }

        [Fact]
        public async Task Mutation_ShouldSucceed_WithGoodRequest()
        {
            //Arrange
            var request = TestHelpers.TestDataProvider.GetTestData<EnrolToAzureB2CCommandRequest>("Account//EnrolToAzureB2CCommandRequest");

            //Act
            var result = await _mutation.VerificationEmail(request);

            //Assert
            Assert.Equal("Verification Email Request Sent Successfully", result);
        }

        [Fact]
        public async Task EnrolCustomer_ShouldReturnException_FailureFromAPI()
        {
            //Arrange
            var request = TestHelpers.TestDataProvider.GetTestData<EnrolToAzureB2CCommandRequest>("Account//EnrolToAzureB2CCommandRequest");
            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity };
            _connector.Setup(x => x.SendAsync<string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _mutation.VerificationEmail(request));
        }
    }
}
