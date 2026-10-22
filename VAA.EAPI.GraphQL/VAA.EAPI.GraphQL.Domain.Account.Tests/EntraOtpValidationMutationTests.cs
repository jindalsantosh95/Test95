using HotChocolate;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class EntraOtpValidationMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IEntraServicesProvider<EntraOtpValidationAPIResponse> _entraServicesOtpValidation;
        private readonly IOTPMutationProvider<string> _otpMutationProvider;
        private readonly EntraOtpValidationMutation _mutation;

        public EntraOtpValidationMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            if (httpClientFactoryFixture is null) throw new ArgumentNullException(nameof(httpClientFactoryFixture));

            Setup.CreateTestEnvironmentSettings();

            var connector = new HttpConnector(NullLogger<HttpConnector>.Instance, httpClientFactoryFixture.Current);
            var tokenProvider = new TestOAuthTokenProvider();

            _entraServicesOtpValidation = new EntraServicesProvider<EntraOtpValidationAPIResponse>(tokenProvider, connector);
            _otpMutationProvider = new OTPMutationProvider<string>(tokenProvider, connector);

            _mutation = new EntraOtpValidationMutation(_entraServicesOtpValidation, NullLogger<EntraOtpValidationMutation>.Instance, _otpMutationProvider);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EntraOtpValidationMutation(_entraServicesOtpValidation, null!, _otpMutationProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenEntraServicesOtpValidationObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EntraOtpValidationMutation(null!, NullLogger<EntraOtpValidationMutation>.Instance, _otpMutationProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenOtpMutationProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EntraOtpValidationMutation(_entraServicesOtpValidation, NullLogger<EntraOtpValidationMutation>.Instance, null!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task EntraOtpValidate_ShouldReturnGraphQlError_WhenOtpValueIsNullOrEmpty(string? otp)
        {
            Setup.CreateTestEnvironmentSettings();

            var request = new EntraOtpValidationRequest
            {
                Token = "continuation-token",
                OTP = otp!,
                ValidationRequestType = OtpValidationRequestType.SIGNUP
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EntraOtpValidate(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal(HttpStatusCode.InternalServerError.ToString(), error.Code);
            Assert.Equal("Error while validating otp", error.Message);
            Assert.Contains("OTP", error.Extensions!["Exception"]?.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task EntraOtpValidate_ShouldReturnGraphQlError_WhenTokenValueIsNullOrEmpty(string? token)
        {
            Setup.CreateTestEnvironmentSettings();

            var request = new EntraOtpValidationRequest
            {
                Token = token!,
                OTP = "123456",
                ValidationRequestType = OtpValidationRequestType.SIGNUP
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EntraOtpValidate(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal(HttpStatusCode.InternalServerError.ToString(), error.Code);
            Assert.Equal("Error while validating otp", error.Message);
            Assert.Contains("Token", error.Extensions!["Exception"]?.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task EntraOtpValidate_ShouldReturnTokenResponse_WhenEntraServiceSucceeds()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/entra-otp-validation-success");

            var request = new EntraOtpValidationRequest
            {
                Token = "continuation-token",
                OTP = "123456",
                ValidationRequestType = OtpValidationRequestType.SIGNIN
            };

            var result = await _mutation.EntraOtpValidate(request);

            Assert.NotNull(result);
            Assert.Equal("success-continuation-token", result.ContinuationToken);
            Assert.Equal("Bearer", result.TokenType);
            Assert.Equal("openid profile", result.Scope);
            Assert.Equal(3600, result.ExpiresIn);
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal("id-token", result.IdToken);
        }

        [Fact]
        public async Task EntraOtpValidate_ShouldReturnAccountExists_WhenCustomOtpValidationSucceeds()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/entra-otp-validation-fallback");

            var request = new EntraOtpValidationRequest
            {
                Token = CreateJwtTokenWithEmail("test@test.com", isCustomOtpContinuationToken: true),
                OTP = "12345678",
                ValidationRequestType = OtpValidationRequestType.SIGNUP
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EntraOtpValidate(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal(Application.DuplicateEmailErrorMessage, error.Message);
            Assert.Equal(Application.DuplicateEmailErrorCode, error.Extensions!["code"]);
            Assert.Equal(Application.DuplicateEmailErrorMessage, error.Extensions["message"]);
        }

        [Fact]
        public async Task EntraOtpValidate_ShouldReturnAccountExists_WhenCustomContinuationTokenIsValid()
        {
            Setup.CreateTestEnvironmentSettings();

            var request = new EntraOtpValidationRequest
            {
                Token = CreateJwtTokenWithEmail("test@test.com", isCustomOtpContinuationToken: true),
                OTP = "12345678",
                ValidationRequestType = OtpValidationRequestType.SIGNUP
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EntraOtpValidate(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal(Application.DuplicateEmailErrorMessage, error.Message);
            Assert.Equal(Application.DuplicateEmailErrorCode, error.Extensions!["code"]);
            Assert.Equal(Application.DuplicateEmailErrorMessage, error.Extensions["message"]);
        }

        [Fact]
        public async Task EntraOtpValidate_ShouldReturnUnauthorized_WhenCustomContinuationTokenIsUnsigned()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/otp-token-validation-error");

            var request = new EntraOtpValidationRequest
            {
                Token = CreateJwtTokenWithEmail("test@test.com", isCustomOtpContinuationToken: true, signToken: false),
                OTP = "12345678",
                ValidationRequestType = OtpValidationRequestType.SIGNUP
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EntraOtpValidate(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal(HttpStatusCode.Unauthorized.ToString(), error.Code);
            Assert.Equal("OTP validation failed", error.Message);
        }

        [Fact]
        public async Task EntraOtpValidate_ShouldReturnUnauthorized_WhenCustomContinuationTokenSignatureIsInvalid()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/otp-token-validation-error");

            var request = new EntraOtpValidationRequest
            {
                Token = CreateJwtTokenWithEmail("test@test.com", isCustomOtpContinuationToken: true, signingKey: "DifferentTestkeySRECDSADSADSAREWEF76545$==+"),
                OTP = "12345678",
                ValidationRequestType = OtpValidationRequestType.SIGNUP
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EntraOtpValidate(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal(HttpStatusCode.Unauthorized.ToString(), error.Code);
            Assert.Equal("OTP validation failed", error.Message);
        }

        [Fact]
        public async Task EntraOtpValidate_ShouldReturnUnauthorized_WhenCustomContinuationTokenIssuerIsInvalid()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/otp-token-validation-error");

            var request = new EntraOtpValidationRequest
            {
                Token = CreateJwtTokenWithEmail("test@test.com", isCustomOtpContinuationToken: true, issuer: "unexpected-issuer"),
                OTP = "12345678",
                ValidationRequestType = OtpValidationRequestType.SIGNUP
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EntraOtpValidate(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal(HttpStatusCode.Unauthorized.ToString(), error.Code);
            Assert.Equal("OTP validation failed", error.Message);
        }

        [Fact]
        public async Task EntraOtpValidate_ShouldReturnAccountExists_WhenLegacyCustomContinuationTokenHasNoIssuer()
        {
            Setup.CreateTestEnvironmentSettings();

            var request = new EntraOtpValidationRequest
            {
                Token = CreateJwtTokenWithEmail("test@test.com", isCustomOtpContinuationToken: true, includeIssuer: false),
                OTP = "12345678",
                ValidationRequestType = OtpValidationRequestType.SIGNUP
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EntraOtpValidate(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal(Application.DuplicateEmailErrorMessage, error.Message);
            Assert.Equal(Application.DuplicateEmailErrorCode, error.Extensions!["code"]);
            Assert.Equal(Application.DuplicateEmailErrorMessage, error.Extensions["message"]);
        }

        [Fact]
        public async Task EntraOtpValidate_ShouldReturnApiErrorDetails_WhenCustomOtpValidationReturnsHttpErrorWithErrorDetails()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/entra-otp-validation-validateotp-error");

            var request = new EntraOtpValidationRequest
            {
                Token = CreateJwtTokenWithEmail("test@test.com", isCustomOtpContinuationToken: true),
                OTP = "12345678",
                ValidationRequestType = OtpValidationRequestType.SIGNUP
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EntraOtpValidate(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("OTP validation failed", error.Message);
            Assert.Equal("E1", error.Extensions!["code"]);
            Assert.Equal("Invalid OTP", error.Extensions!["message"]);
            Assert.Equal("IdentityMS", error.Extensions!["source"]);
        }

        private static string CreateJwtTokenWithEmail(
            string emailAddress,
            bool isCustomOtpContinuationToken = false,
            string? signingKey = null,
            bool signToken = true,
            string? issuer = null,
            bool includeIssuer = true)
        {
            var handler = new JwtSecurityTokenHandler();
            var claims = new List<Claim> { new("emailAddress", emailAddress) };

            if (isCustomOtpContinuationToken)
                claims.Add(new Claim("continuationTokenSource", "custom_otp"));

            var signingCredentials = signToken
                ? new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey ?? Setup.TestIdentitySigningKey)),
                    SecurityAlgorithms.HmacSha256)
                : null;

            var jwt = new JwtSecurityToken(
                issuer: includeIssuer ? issuer ?? Setup.TestIdentityActivationTokenIssuer : null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: signingCredentials);

            return handler.WriteToken(jwt);
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("dummy-token");
        }
    }
}
