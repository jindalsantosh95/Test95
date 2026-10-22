using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class OtpHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public OtpHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IEntraService, EntraService>()
                .AddSingleton<OtpHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task SubmitOTP_ReturnsEntraContinuationToken_WhenJourneyIsSignup()
        {
            var handler = _serviceProvider.GetRequiredService<OtpHandler>();

            var result = await handler.SubmitOTP(
                new OtpRequest
                {
                    ContinuationToken = "start-continuation-token",
                    OtpCode = "12345678",
                    UrlEntraJourney = "signup"
                },
                CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.StartsWith("X0ZwcEM5ak9WWDB1WkNFcnYtcVlVYXR5UkpyVTA3di05NnZOVTRaMEdqVlElM2QlM2Q", result.Value.ContinuationToken);
            Assert.Null(result.Errors);
        }

        [Fact]
        public async Task CreateToken_UsesHs256_WhenCreatingCustomOtpContinuationToken()
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();

            var token = await ValidationTokenHelper.CreateToken("test@example.com", isContinuationToken: true);
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.Equal(SecurityAlgorithms.HmacSha256, jwtToken.Header.Alg);
        }
    }
}
