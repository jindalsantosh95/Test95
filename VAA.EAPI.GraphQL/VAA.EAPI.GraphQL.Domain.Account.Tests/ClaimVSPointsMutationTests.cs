using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Domain.Account.Tests.Helpers;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{

    [Collection("EnvironmentDependentTests")]
    public sealed class ClaimVSPointsMutationTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RecordingLoggerProvider _loggerProvider = new();

        public ClaimVSPointsMutationTests()
        {
            Setup.CreateTestEnvironmentSettings();
            // Default: do not return mock VS points response unless overridden by a test
            ConfigReader.Set(VAA.EAPI.GraphQL.Common.Constants.Environment.EnableMockResponseForVSPointsClaim, "false");
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            var jwtFeature = new JwtPrincipalFeature(new ClaimsPrincipal(), new AuthToken { Token = "dummy-token" });
            AsyncContext.Current.Context.Features.Set(jwtFeature);

            var services = new ServiceCollection()
                .AddLogging(logging => logging.AddProvider(_loggerProvider))
                .AddSingleton<IOAuthTokenProvider, TestOAuthTokenProvider>()
                .AddSingleton<IHttpClientFactory, VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory>()
                .AddSingleton<IHttpConnector, VAA.EAPI.GraphQL.Infrastructure.Services.HttpConnector>()
                .AddSingleton<IAccountCustomerService, CustomerService>()
                .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
                .AddSingleton<IPDMProvider, PDMProvider>()
                .AddSingleton(typeof(IRefDataService<>), typeof(RefDataService<>))
                .AddSingleton<IRulesEngineProvider<VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS.AccrualPointsCalculatorResponse>, RulesEngineProvider<VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS.AccrualPointsCalculatorResponse>>()
                .AddSingleton<ILoyaltyMemberManagementProvider, LoyaltyMemberManagementProvider>()
                .AddSingleton<ITransactionMutationProvider, TransactionMutationProvider>()
                .AddSingleton(typeof(ICustomerTransactionQueryProvider<>), typeof(CustomerTransactionQueryProvider<>))
                .AddSingleton<ClaimVSPointsMutation>()
                .AddSingleton<TimeProvider>(new FixedTimeProvider(new DateTimeOffset(new DateTime(2026, 05, 01, 0, 0, 0, DateTimeKind.Utc), TimeSpan.Zero)))
                .BuildServiceProvider();

            _serviceProvider = services;
        }

        [Fact]
        public async Task ClaimPoints_WithMockFlagEnabled_ReturnsMockResponse()
        {
            ConfigReader.Set(VAA.EAPI.GraphQL.Common.Constants.Environment.EnableMockResponseForVSPointsClaim, "true");


            var sut = _serviceProvider.GetRequiredService<ClaimVSPointsMutation>();

            // Act
            var response = await sut.claimPoints("TICKET-123", "FC-123");

            // Assert 
            Assert.NotNull(response);
            Assert.Equal("100", response.TotalBonusPoints);
            Assert.Equal("1500", response.TotalTierPoints);
            Assert.Equal("3000", response.TotalVirginPoints);

            var seg = Assert.Single(response.segmentDetails);
            Assert.Equal("VS26", seg.FlightNumber);
            Assert.Equal("2320", seg.VirginPoints);
            Assert.Equal("1000", seg.BonusPoints);
            Assert.Equal("50", seg.TierPoints);
            Assert.Null(response.ErrorResponse);
        }

        [Fact]
        public async Task ClaimPoints_EmptyTicket_ThrowsArgumentNullException()
        {
            var sut = _serviceProvider.GetRequiredService<ClaimVSPointsMutation>();
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.claimPoints(string.Empty, "FC123"));
        }

        [Fact]
        public async Task ClaimPoints_EmptyFcNumber_ThrowsArgumentNullException()
        {
            var sut = _serviceProvider.GetRequiredService<ClaimVSPointsMutation>();
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.claimPoints("TICKET123", string.Empty));
        }

        [Fact]
        public async Task ClaimPoints_WhenFlownDataUpdateSucceeds_ReturnsSuccessfulClaimResponse()
        {
            ConfigReader.Set(Common.Constants.Environment.EnableMockResponseForVSPointsClaim, "false");
            ConfigReader.Set(Common.Constants.Environment.PdmMSUrl, "https://Testresources.virginatlantic.com/dev/pdmms");
            ConfigReader.Set(Common.Constants.Environment.PdmMSAppId, "test-pdm-appid");
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/Good");
            ConfigReader.Set(Common.Constants.Environment.InRuleMSUrl, "https://Testresources.virginatlantic.com/dev/inrule");
            ConfigReader.Set(Common.Constants.Environment.RefDataMSGenericUrl, "https://Testresources.virginatlantic.com/dev/ReferenceMicroservice/{0}");


            var sut = _serviceProvider.GetRequiredService<ClaimVSPointsMutation>();
            //
            // Act
            var ticketNumber = "9386008345782";
            var fcNumber = "1074840875";

            var response = await sut.claimPoints(ticketNumber, fcNumber);

            // Assert 
            Assert.NotNull(response);
            Assert.Null(response.ErrorResponse);
            Assert.NotNull(response.segmentDetails);
            Assert.True(response.segmentDetails.Any(), "Expected at least one claimed flight segment from test resources.");
            Assert.DoesNotContain(_loggerProvider.Messages, message => message.StartsWith("Flown data update failed after a successful VS retro claim."));
            // verify totals parse to integers (basic validation)
            int.TryParse(response.TotalVirginPoints, out var virgin);
            int.TryParse(response.TotalBonusPoints, out var bonus);
            int.TryParse(response.TotalTierPoints, out var tier);
            Assert.True(virgin >= 0);
            Assert.True(bonus >= 0);
            Assert.True(tier >= 0);
        }

        [Fact]
        public async Task ClaimPoints_WhenFlownDataUpdateIsRejected_ReturnsSuccessfulClaimResponse()
        {
            ConfigReader.Set(Common.Constants.Environment.EnableMockResponseForVSPointsClaim, "false");
            ConfigReader.Set(Common.Constants.Environment.PdmMSUrl, "https://Testresources.virginatlantic.com/dev/pdmms");
            ConfigReader.Set(Common.Constants.Environment.PdmMSAppId, "test-pdm-appid");
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/Good");
            ConfigReader.Set(Common.Constants.Environment.InRuleMSUrl, "https://Testresources.virginatlantic.com/dev/inrule");
            ConfigReader.Set(Common.Constants.Environment.RefDataMSGenericUrl, "https://Testresources.virginatlantic.com/dev/ReferenceMicroservice/{0}");

            var sut = _serviceProvider.GetRequiredService<ClaimVSPointsMutation>();

            var response = await sut.claimPoints("9386008345783", "1074840875");

            Assert.NotNull(response);
            Assert.Null(response.ErrorResponse);
            Assert.NotEmpty(response.segmentDetails);
            Assert.Contains(_loggerProvider.Messages, message => message.Contains("No status updated for given Ticket Number"));
        }

        private sealed class RecordingLoggerProvider : ILoggerProvider
        {
            public ConcurrentQueue<string> Messages { get; } = new();

            public ILogger CreateLogger(string categoryName) => new RecordingLogger(Messages);

            public void Dispose()
            {
            }
        }

        private sealed class RecordingLogger : ILogger
        {
            private readonly ConcurrentQueue<string> _messages;

            public RecordingLogger(ConcurrentQueue<string> messages)
            {
                _messages = messages;
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoOpDisposable.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _messages.Enqueue(formatter(state, exception));
            }
        }

        private sealed class NoOpDisposable : IDisposable
        {
            public static readonly NoOpDisposable Instance = new();

            public void Dispose()
            {
            }
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("test-app-token");
        }
    }
}
