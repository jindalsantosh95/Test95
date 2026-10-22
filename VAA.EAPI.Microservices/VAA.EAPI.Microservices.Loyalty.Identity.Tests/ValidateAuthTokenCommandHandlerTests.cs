using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Validation;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class ValidateAuthTokenCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly AuthTokenValidator _tokenValidator;
        private readonly VerifyMemberStatusRepository _verifyMemberStatusRepository;
        private readonly ValidateAuthTokenCommandHandler _handler;
        private readonly ILogger<ValidateAuthTokenCommand> _logger;
        private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();

        public ValidateAuthTokenCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();

            var httpConnector = new HttpConnector(NullLogger<HttpConnector>.Instance, httpClientFactoryFixture.Current);
            var getAdUserDetailsService = new GetAdUserDetailsService(httpConnector);

            _tokenValidator = new AuthTokenValidator(NullLogger<AuthTokenValidator>.Instance);
            _verifyMemberStatusRepository = new VerifyMemberStatusRepository(getAdUserDetailsService, NullLogger<VerifyMemberStatusRepository>.Instance);
            _logger = NullLogger<ValidateAuthTokenCommand>.Instance;
            _handler = new ValidateAuthTokenCommandHandler(_tokenValidator, _logger, _verifyMemberStatusRepository);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenValidatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ValidateAuthTokenCommandHandler(null, _logger, _verifyMemberStatusRepository));
        }

        [Fact]
        public async Task Handler_ShouldReturnError_ForFailedValidation()
        {
            var request = TestDataProvider.GetTestData<ValidateAuthTokenCommand>("IdentityMS//ValidateAuthTokenCommandRequest");
            request.Token = CreateToken(new Claim("memberStatus", "SUSPENDED"));

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.NotNull(result.Errors);
            Assert.Equal("Provided token is either Invalid or Expired.", result.Errors.FirstOrDefault().Message);
            Assert.Equal("10160", result.Errors.FirstOrDefault().Code);
        }

        [Fact]
        public async Task Handler_ShouldReturnFCNumber_ForSuccessValidation()
        {
            var request = TestDataProvider.GetTestData<ValidateAuthTokenCommand>("IdentityMS//ValidateAuthTokenCommandRequest");
            request.Token = CreateToken(
                new Claim("memberStatus", "ACTIVE"),
                new Claim("memberType", "Individual"),
                new Claim("EmailAddress", "kilosappoundsap@outlook.com"),
                new Claim("name", "kilosap"),
                new Claim("family_name", "poundsqp"),
                new Claim("flyingclubNumber", "9106430888"));

            var result = await _handler.Handle(request, CancellationToken.None);
            var claim = DeserializeResponse(result.Value);

            Assert.Equal(EAPI.Domain.Entities.Common.Status.OK, result.Status);
            Assert.Equal("9106430888", claim.FcNumber);
        }

        [Fact]
        public async Task Handler_ShouldPopulateMemberStatusAndFcNumber_FromRepository_WhenEmailAddressClaimIsMissing()
        {
            ConfigReader.Set("GraphAPIUrl", "https://graph.test.com/Success/");

            var request = TestDataProvider.GetTestData<ValidateAuthTokenCommand>("IdentityMS//ValidateAuthTokenCommandRequest");
            request.Token = CreateToken(
                new Claim("oid", "oid-123"),
                new Claim("name", "kilosap"),
                new Claim("family_name", "poundsqp"));

            var result = await _handler.Handle(request, CancellationToken.None);
            var claim = DeserializeResponse(result.Value);

            Assert.Equal(EAPI.Domain.Entities.Common.Status.OK, result.Status);
            Assert.Equal("ACTIVE", claim.MemberStatus);
            Assert.Equal("9106430888", claim.FcNumber);
            Assert.False(string.IsNullOrWhiteSpace(claim.EmailAddress));
        }

        [Fact]
        public async Task Handler_ShouldNotOverwriteMemberStatusAndFcNumber_WhenAlreadyPresent_AndEmailAddressClaimIsMissing()
        {
            ConfigReader.Set("GraphAPIUrl", "https://graph.test.com/Success/");

            var request = TestDataProvider.GetTestData<ValidateAuthTokenCommand>("IdentityMS//ValidateAuthTokenCommandRequest");
            request.Token = CreateToken(
                new Claim("memberStatus", "TOKEN_ACTIVE"),
                new Claim("flyingclubNumber", "9990001112"),
                new Claim("oid", "oid-123"),
                new Claim("name", "kilosap"),
                new Claim("family_name", "poundsqp"));

            var result = await _handler.Handle(request, CancellationToken.None);
            var claim = DeserializeResponse(result.Value);

            Assert.Equal(EAPI.Domain.Entities.Common.Status.OK, result.Status);
            Assert.Equal("TOKEN_ACTIVE", claim.MemberStatus);
            Assert.Equal("9990001112", claim.FcNumber);
            Assert.False(string.IsNullOrWhiteSpace(claim.EmailAddress));
        }

        [Fact]
        public async Task Handler_ShouldThrowNullException_ForNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null, CancellationToken.None));
        }

        private static TokenClaimResponse DeserializeResponse(string serialized)
        {
            return JsonSerializer.Deserialize<TokenClaimResponse>(serialized, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private static string CreateToken(params Claim[] claims)
        {
            var allClaims = claims.ToList();
            allClaims.Add(new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds().ToString()));
            return JwtSecurityTokenHandler.WriteToken(new JwtSecurityToken(claims: allClaims));
        }
    }
}
