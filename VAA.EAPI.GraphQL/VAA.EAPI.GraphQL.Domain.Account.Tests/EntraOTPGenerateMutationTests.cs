using HotChocolate;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class EntraOTPGenerateMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IAccountCustomerService _customerService;
        private readonly IOTPMutationProvider<string> _otpMutationProvider;
        private readonly EntraOTPGenerationMutation _mutation;

        public EntraOTPGenerateMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            if (httpClientFactoryFixture is null) throw new ArgumentNullException(nameof(httpClientFactoryFixture));

            var connector = new HttpConnector(NullLogger<HttpConnector>.Instance, httpClientFactoryFixture.Current);
            var tokenProvider = new TestOAuthTokenProvider();

            _customerService = new CustomerService(tokenProvider, connector);
            _otpMutationProvider = new OTPMutationProvider<string>(tokenProvider, connector);

            _mutation = new EntraOTPGenerationMutation(_customerService, NullLogger<EntraOTPGenerationMutation>.Instance, _otpMutationProvider);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenCustomerServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EntraOTPGenerationMutation(null!, NullLogger<EntraOTPGenerationMutation>.Instance, _otpMutationProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EntraOTPGenerationMutation(_customerService, null!, _otpMutationProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenOtpMutationProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EntraOTPGenerationMutation(_customerService, NullLogger<EntraOTPGenerationMutation>.Instance, null!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SignUp_ShouldReturnUnauthorized_WhenEmailIdIsNullOrWhiteSpace(string? emailId)
        {
            Setup.CreateTestEnvironmentSettings();

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.SignUp(emailId!));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("G013", error.Extensions!["code"]);
            Assert.Equal("Email id in the request is Invalid or Missing", error.Extensions!["message"]);
        }

        [Fact]
        public async Task SignUp_ShouldGenerateEntraOtp_WhenCustomerDoesNotExist()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Common.Constants.Environment.CustomerMsUrl, "https://Testresources.virginatlantic.com/dev/microservices/null");

            var result = await _mutation.SignUp("Test@Example.com");

            Assert.Equal("entra-token", result);
        }

        [Fact]
        public async Task SignUp_ShouldGenerateOtp_WhenCustomerAlreadyExists()
        {
            Setup.CreateTestEnvironmentSettings();

            var result = await _mutation.SignUp("Test@Example.com");

            Assert.True(new JwtSecurityTokenHandler().CanReadToken(result));
            Assert.True(Domain.Account.Helpers.TokenHelper.IsCustomOtpContinuationToken(result));
        }

        [Fact]
        public async Task SignUp_ShouldReturnApiErrorDetails_WhenOtpProviderReturnsHttpErrorWithErrorDetails()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/entra-signup-error");

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.SignUp("Test@Example.com"));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("OTP Generation failed", error.Message);
            Assert.Equal("E1", error.Extensions!["code"]);
            Assert.Equal("Invalid email", error.Extensions!["message"]);
            Assert.Equal("IdentityMS", error.Extensions!["source"]);
        }

        [Fact]
        public async Task SignUp_ShouldReturnUnauthorized_WhenCustomerLookupReturnsHttpError()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Common.Constants.Environment.CustomerMsUrl, "https://Testresources.virginatlantic.com/dev/microservices/entra-signup-error");

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.SignUp("Test@Example.com"));
            var error = Assert.Single(exception.Errors);

            Assert.Equal(Common.Constants.Application.InvalidorMissingEmailIDErrorCode, error.Extensions!["code"]);
            Assert.Equal(Common.Constants.Application.InvalidorMissingEmailIDErrorMessage, error.Extensions!["message"]);
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("dummy-token");
        }
    }
}

