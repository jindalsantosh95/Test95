using HotChocolate;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class PasswordResetStartMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IEntraServicesProvider<string> _entraServicesRefreshToken;
        private readonly IAccountCustomerService _customerService;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<PasswordResetStartMutation> _loggerMutation;
        private readonly ILogger<HttpConnector> _logger;
        private readonly PasswordResetStartMutation _mutation;

        public PasswordResetStartMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _tokenProvider = new TestOAuthTokenProvider();
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerMutation = Microsoft.Extensions.Logging.Abstractions.NullLogger<PasswordResetStartMutation>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);

            _entraServicesRefreshToken = new EntraServicesProvider<string>(_tokenProvider, _httpConnector);
            _customerService = new CustomerService(_tokenProvider, _httpConnector);
            _mutation = new PasswordResetStartMutation(_entraServicesRefreshToken, _customerService, _loggerMutation);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenB2CProvideObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PasswordResetStartMutation(_entraServicesRefreshToken, _customerService, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenProductRefServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PasswordResetStartMutation(null, _customerService, _loggerMutation));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PasswordResetStartMutation(_entraServicesRefreshToken, null, _loggerMutation));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullRequest()
        {
            //Arrange
            var passwordResetStartRequest = TestDataProvider.GetTestData<PasswordResetStartRequest>("Account//PasswordResetStartRequest");
            passwordResetStartRequest.FcNumberOrEmail = null;

            //Act
            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.ResetPasswordInitiate(passwordResetStartRequest));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForEmptyRequest()
        {
            //Arrange
            var passwordResetStartRequest = TestDataProvider.GetTestData<PasswordResetStartRequest>("Account//PasswordResetStartRequest");
            passwordResetStartRequest.FcNumberOrEmail = string.Empty;

            //Act
            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.ResetPasswordInitiate(passwordResetStartRequest));
        }

        [Fact]
        public async Task Mutation_Success_with_email()
        {
            //Arrange
            var passwordResetStartRequest = new PasswordResetStartRequest { FcNumberOrEmail = "test@example.com" };
            ConfigReader.Set(Common.Constants.Environment.CustomerMsUrl, "https://Testresources.virginatlantic.com/dev/microservices");

            //Act
            var result = await _mutation.ResetPasswordInitiate(passwordResetStartRequest);

            //Assert
            Assert.NotNull(result.ContinuationToken);
            Assert.IsType<InitiatePasswordResetResponse>(result);
            Assert.IsType<string>(result.ContinuationToken);
            Assert.StartsWith("AQAB", result.ContinuationToken);
        }

        [Fact]
        public async Task Mutation_Success_with_FCNumber()
        {
            //Arrange
            var passwordResetStartRequest = TestDataProvider.GetTestData<PasswordResetStartRequest>("Account//PasswordResetStartRequest");
            ConfigReader.Set(Common.Constants.Environment.CustomerMsUrl, "https://Testresources.virginatlantic.com/dev/microservices");

            //Act
            var result = await _mutation.ResetPasswordInitiate(passwordResetStartRequest);

            //Assert
            Assert.NotNull(result.ContinuationToken);
            Assert.IsType<InitiatePasswordResetResponse>(result);
            Assert.IsType<string>(result.ContinuationToken);
            Assert.StartsWith("AQAB", result.ContinuationToken);
        }

        [Fact]
        public async Task Mutation_Success_WhenCustomerStatusIsInactive()
        {
            var passwordResetStartRequest = TestDataProvider.GetTestData<PasswordResetStartRequest>("Account//PasswordResetStartRequest");
            ConfigReader.Set(Common.Constants.Environment.CustomerMsUrl, "https://Testresources.virginatlantic.com/dev/microservices/inactive");

            var result = await _mutation.ResetPasswordInitiate(passwordResetStartRequest);

            Assert.NotNull(result.ContinuationToken);
            Assert.IsType<InitiatePasswordResetResponse>(result);
            Assert.IsType<string>(result.ContinuationToken);
            Assert.StartsWith("AQAB", result.ContinuationToken);
        }

        [Fact]
        public async Task Mutation_ShouldReturnInactiveMemberError_WhenCustomerStatusIsSuspended()
        {
            var passwordResetStartRequest = TestDataProvider.GetTestData<PasswordResetStartRequest>("Account//PasswordResetStartRequest");
            ConfigReader.Set(Common.Constants.Environment.CustomerMsUrl, "https://Testresources.virginatlantic.com/dev/microservices/suspended");

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.ResetPasswordInitiate(passwordResetStartRequest));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("Account member password reset failed", error.Message);
            Assert.Equal("601", error.Extensions!["code"]);
            Assert.Equal("Account did not meet the required status to reset password", error.Extensions!["message"]);
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("abc");
        }
    }
}
