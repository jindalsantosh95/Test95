using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Diagnostics.Telemetry;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class EntraAccountSignInCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RecordingCustomEventTracker _customEventTracker = new();

        private sealed class RecordingCustomEventTracker : ICustomEventTracker
        {
            public int EventCount { get; private set; }
            public string EventName { get; private set; } = string.Empty;
            public Dictionary<string, string> EventProperties { get; private set; } = new();

            public void TrackEvent(string eventName, IDictionary<string, string> properties)
            {
                EventCount++;
                EventName = eventName;
                EventProperties = new Dictionary<string, string>(properties);
            }
        }

        public EntraAccountSignInCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ICustomEventTracker>(_customEventTracker)
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IMediator>(serviceProvider => new Mediator(serviceProvider))
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IUpdateAdUserService, UpdateAdUserService>()
                .AddSingleton<IGetAdUserDetailsService, GetAdUserDetailsService>()
                .AddSingleton<IVerifyMemberStatusRepository, VerifyMemberStatusRepository>()
                .AddSingleton<IEntraService, EntraService>()
                .AddSingleton<AccountSignInHandler>()
                .BuildServiceProvider();
        }

        private AccountSignInHandler ResolveHandler()
            => _serviceProvider.GetRequiredService<AccountSignInHandler>();

        private static void SetEntraApiUrlDomain(string value)
            => ConfigReader.Set(Identity.Constants.Environment.EntraAPIurlDomain, value);

        private static void SetGraphApiUrl(string value = "https://graph.test.com/Success/")
            => ConfigReader.Set(Identity.Constants.Environment.GraphAPIUrl, value);

        [Fact]
        public async Task AccountSignIn_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            var handler = ResolveHandler();

            var result = await handler.AccountSignIn(null, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Request is null", result.Errors.First().Message);
            Assert.Equal(1, _customEventTracker.EventCount);
            Assert.Equal("EntraDiags", _customEventTracker.EventName);
            Assert.Equal(nameof(AccountSignInHandler), _customEventTracker.EventProperties["Handler"]);
            Assert.Equal("Missing continuation token", _customEventTracker.EventProperties["Message"]);
            Assert.Equal("Information", _customEventTracker.EventProperties["Severity"]);
        }

        [Fact]
        public async Task AccountSignIn_ShouldReturnBadRequest_WhenEmailIsMissing()
        {
            var handler = ResolveHandler();
            var request = new AccountSignInRequest
            {
                Email = "  ",
                Password = "password",
                OTP = "123456",
                FlyingClubNumber = "1501133589"
            };

            var result = await handler.AccountSignIn(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("400", result.Errors.First().Code);
            Assert.Equal("invalid_request", result.Errors.First().StatusText);
            Assert.Equal("Username is required", result.Errors.First().Message);
        }

        [Fact]
        public async Task AccountSignIn_ShouldReturnUserNotFound_WhenMemberNotInDirectory()
        {
            SetEntraApiUrlDomain("avateunloyaltyfcb2c.ciamlogin.com/avateunloyaltyfcb2c.onmicrosoft.com/NoUser");

            var handler = ResolveHandler();
            var request = new AccountSignInRequest { Email = "testuser", Password = "password", OTP = "123456", FlyingClubNumber = "1501133589" };

            var result = await handler.AccountSignIn(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("601", result.Errors.First().Code);
        }

        [Fact]
        public async Task AccountSignIn_ShouldReturnBusinessError_WhenChallengeTypeIsInvalid()
        {
            SetEntraApiUrlDomain("avateunloyaltyfcb2c.ciamlogin.com/avateunloyaltyfcb2c.onmicrosoft.com/InvalidChallenge");
            SetGraphApiUrl();

            var handler = ResolveHandler();
            var request = new AccountSignInRequest { Email = "deventratest@test.com", Password = "password", OTP = "123456", FlyingClubNumber = "1501133589" };

            var result = await handler.AccountSignIn(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("901007", result.Errors.First().Code);
        }

        [Fact]
        public async Task AccountSignIn_ShouldReturnTokenObject_WhenSignInSucceeds()
        {
            SetEntraApiUrlDomain("avateunloyaltyfcb2c.ciamlogin.com/avateunloyaltyfcb2c.onmicrosoft.com");
            SetGraphApiUrl();

            var handler = ResolveHandler();
            var request = new AccountSignInRequest { Email = "deventratest@test.com", Password = "password", OTP = null, FlyingClubNumber = "1501133589" };

            var result = await handler.AccountSignIn(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(Status.OK, result.Status);
            Assert.NotNull(result.Value);
            Assert.StartsWith("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IjJa", result.Value.AccessToken);
            Assert.StartsWith("0.AV8AWyS-BgKh8kmG/OTK9n8EegMAAAAAAAAAwAAAAAAAAAAzAAA", result.Value.RefreshToken);
            Assert.StartsWith("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IjJa", result.Value.IdToken);
            Assert.Equal(3600, result.Value.ExpiresIn);
            Assert.Equal("Bearer", result.Value.TokenType);
        }

        [Fact]
        public async Task AccountSignIn_ShouldReturnContinuationToken_WhenMfaIsRequired()
        {
            SetEntraApiUrlDomain("avateunloyaltyfcb2c.ciamlogin.com/MFA/avateunloyaltyfcb2c.onmicrosoft.com");
            SetGraphApiUrl();

            var handler = ResolveHandler();
            var request = new AccountSignInRequest { Email = "deventratest@test.com", Password = "password", OTP = null, FlyingClubNumber = "1501133589" };

            var result = await handler.AccountSignIn(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(Status.OK, result.Status);
            Assert.NotNull(result.Value);
            Assert.Null(result.Value.AccessToken);
            Assert.Null(result.Value.IdToken);
            Assert.StartsWith("X0ZwcEM5ak9WWDB1WkNFcnYtcVlVYXR5UkpyVTA3di05NnZOVTRaMEdqVlE", result.Value.ContinuationToken);
        }
    }
}
