using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.WingsMemberAssessment;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.WingsGoldMemberAssessment;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.CustomerTransactionMs;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.RuleEngineMs;
using Xunit;
using CoreXunitHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class WingsMemberAssessmentCommandHandlerTest
    {
        private readonly IServiceProvider _serviceProvider;

        public WingsMemberAssessmentCommandHandlerTest()
        {
            ConfigReader.Set(
                VAA.EAPI.Common.Constants.Environment.EapiMsClientID,
                "wings-test-client-id");
            ConfigReader.Set(
                VAA.EAPI.Common.Constants.Environment.EapiMSSecret,
                "wings-test-client-secret");
            ConfigReader.Set(
                VAA.EAPI.Common.Constants.Environment.AADAuthority,
                "https://tempuri.org/");
            ConfigReader.Set(
                VAA.EAPI.Common.Constants.Environment.AADTenant,
                "wings-test-tenant");
            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.LoyaltyMbrMgmtAppId,
                "wings-test-client-id");
            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.LoyaltyMbrMgmtClientSecret,
                "wings-test-client-secret");
            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.AzureADEndpoint,
                "https://tempuri.org/");
            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.CustomerTxnMSResourceId,
                "wings-active");

            VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                .Application.LoyaltyMbrMgmtClientSecretCached = "wings-test-client-secret";
            VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                .Application.CustomerTxnMSResourceIdCached = "wings-active";
            VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                .Application.LoyaltyMbrMgmtOcpKeyCached = "test-subscription-key";
            SeedLegacyCustomerTransactionAccessToken();

            AsyncContext.Current = new AsyncContext
            {
                Context = new TestFunctionContext()
            };

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<CoreXunitHttpClientFactory>()
                .AddSingleton<IHttpClientFactory>(serviceProvider =>
                    serviceProvider.GetRequiredService<CoreXunitHttpClientFactory>())
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider, VAA.EAPI.Core.Xunit.Azure.OAuthTokenProvider>()
                .AddSingleton<ICustomerTransactionTokenProvider, CustomerTransactionTokenProvider>()
                .AddSingleton<MockReportingDbServices>()
                .AddSingleton<IReportingDBServices>(serviceProvider =>
                    serviceProvider.GetRequiredService<MockReportingDbServices>())
                .AddSingleton<AwardDocumentServiceQueryHandler>()
                .AddSingleton<FlyingClubMemberDocumentServiceQueryHandler>()
                .AddSingleton<CustomerTransactionDocumentsServiceQueryHandler>()
                .AddSingleton<IRulesEngineService, RulesEngineService>()
                .AddTransient<
                    IWingsMemberAssessmentCommand,
                    WingsMemberAssessmentCommandHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public void ExplicitCast_ShouldMapWingsAssessmentValuesToTierRuleSetEntityRequest()
        {
            var source = new TierRuleSetEntityRequestSource
            {
                Member = new GoldMemberDetails
                {
                    CustomerId = "customer-id",
                    LoyaltyMemberNumber = "member-number"
                },
                VSFlownMiles = 25.5m,
                TotalTierPoints = 50m,
                VSTierPoints = 30m,
                IsMillionMilerAwardEligible = true,
                IsWingsMember = false,
                HasQualifyingVSFlight = true
            };

            var result = (TierRuleSetEntityRequest)source;

            Assert.Equal("customer-id", result.CustomerId);
            Assert.Equal("member-number", result.MemberNumber);
            Assert.Equal("25.5", result.VSFlownMiles);
            Assert.Equal("50", result.RollingPointsTotal);
            Assert.Equal("30", result.VSRollingTierPoints);
            Assert.Equal(bool.TrueString, result.MillionMilerIndicator);
            Assert.Equal(bool.FalseString, result.WingsMember);
            Assert.Equal(bool.TrueString, result.VSFlightInPrevious12Months);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenReportingDbServicesIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new WingsMemberAssessmentCommandHandler(
                    null,
                    GetAwardDocumentQueryHandler(),
                    GetFlyingClubMemberDocumentQueryHandler(),
                    GetCustomerTransactionDocumentsQueryHandler(),
                    GetRulesEngineService(),
                    GetLogger()));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenAwardDocumentServiceQueryHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new WingsMemberAssessmentCommandHandler(
                    _serviceProvider.GetRequiredService<IReportingDBServices>(),
                    null,
                    GetFlyingClubMemberDocumentQueryHandler(),
                    GetCustomerTransactionDocumentsQueryHandler(),
                    GetRulesEngineService(),
                    GetLogger()));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenFlyingClubMemberDocumentServiceQueryHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new WingsMemberAssessmentCommandHandler(
                    _serviceProvider.GetRequiredService<IReportingDBServices>(),
                    GetAwardDocumentQueryHandler(),
                    null,
                    GetCustomerTransactionDocumentsQueryHandler(),
                    GetRulesEngineService(),
                    GetLogger()));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCustomerTransactionDocumentsServiceQueryHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new WingsMemberAssessmentCommandHandler(
                    _serviceProvider.GetRequiredService<IReportingDBServices>(),
                    GetAwardDocumentQueryHandler(),
                    GetFlyingClubMemberDocumentQueryHandler(),
                    null,
                    GetRulesEngineService(),
                    GetLogger()));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new WingsMemberAssessmentCommandHandler(
                    _serviceProvider.GetRequiredService<IReportingDBServices>(),
                    GetAwardDocumentQueryHandler(),
                    GetFlyingClubMemberDocumentQueryHandler(),
                    GetCustomerTransactionDocumentsQueryHandler(),
                    GetRulesEngineService(),
                    null));
        }

        [Fact]
        public async Task Handle_ShouldReturnCompletionMessage()
        {
            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.WingsAssessmentBatchSize,
                "100");

            var reportingDbServices =
                _serviceProvider.GetRequiredService<MockReportingDbServices>();

            reportingDbServices.StagingResult =
                new WingsAssessmentStagingResult(true, "Success", 0);

            reportingDbServices.MemberBatch =
                new WingsAssessmentMemberBatch(
                    Array.Empty<GoldMemberDetails>(),
                    false);

            var handler =
                _serviceProvider.GetRequiredService<
                    IWingsMemberAssessmentCommand>();

            var result = await handler.Handle(
                new WingsMemberAssessmentCommand
                {
                    Id = Guid.NewGuid()
                },
                CancellationToken.None);

            Assert.Equal(
                "Success Processed 0 Gold member row(s) for Wings assessment.",
                result);

            Assert.Equal(
                1,
                reportingDbServices.StageGoldMembersCallCount);

            Assert.Equal(
                1,
                reportingDbServices.GetBatchCallCount);
        }

        [Fact]
        public async Task Handle_ShouldPropagateReportingDatabaseFailures()
        {
            var reportingDbServices =
                _serviceProvider.GetRequiredService<MockReportingDbServices>();

            reportingDbServices.StagingException =
                new InvalidOperationException();

            var handler =
                _serviceProvider.GetRequiredService<
                    IWingsMemberAssessmentCommand>();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(
                    new WingsMemberAssessmentCommand
                    {
                        Id = Guid.NewGuid()
                    },
                    CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldMapAndSendWingsAssessmentRequestForEligibleMember()
        {
            ConfigureSingleMember(
                mockScenario: "active");

            var handler =
                _serviceProvider.GetRequiredService<
                    IWingsMemberAssessmentCommand>();

            var result = await handler.Handle(
                new WingsMemberAssessmentCommand
                {
                    Id = Guid.NewGuid()
                },
                CancellationToken.None);

            Assert.Equal(
                "Success Processed 1 Gold member row(s) for Wings assessment.",
                result);
        }

        [Fact]
        public async Task Handle_ShouldMapWingsAwardEligibleForReevaluationAsWingsMember()
        {
            ConfigureSingleMember(
                mockScenario: "reevaluation");

            var handler =
                _serviceProvider.GetRequiredService<
                    IWingsMemberAssessmentCommand>();

            var result = await handler.Handle(
                new WingsMemberAssessmentCommand
                {
                    Id = Guid.NewGuid()
                },
                CancellationToken.None);

            Assert.Equal("Success Processed 1 Gold member row(s) for Wings assessment.", result);
        }

        [Fact]
        public async Task Handle_ShouldMapActiveWingsAwardExactlyTwelveMonthsOldAsActive()
        {
            ConfigureSingleMember(
                mockScenario: "exact-twelve-months");

            var handler =
                _serviceProvider.GetRequiredService<
                    IWingsMemberAssessmentCommand>();

            var result = await handler.Handle(
                new WingsMemberAssessmentCommand
                {
                    Id = Guid.NewGuid()
                },
                CancellationToken.None);

            Assert.Equal("Success Processed 1 Gold member row(s) for Wings assessment.", result);
        }

        [Fact]
        public async Task Handle_ShouldMapActiveWingsAwardWithinTwelveMonthsAsActive()
        {
            ConfigureSingleMember(
                mockScenario: "within-twelve-months");

            var handler =
                _serviceProvider.GetRequiredService<
                    IWingsMemberAssessmentCommand>();

            var result = await handler.Handle(
                new WingsMemberAssessmentCommand
                {
                    Id = Guid.NewGuid()
                },
                CancellationToken.None);

            Assert.Equal("Success Processed 1 Gold member row(s) for Wings assessment.", result);
        }

        [Fact]
        public async Task Handle_ShouldMapInactiveWingsAwardWithinTwelveMonthsAsInactive()
        {
            ConfigureSingleMember(
                mockScenario: "inactive");

            var handler =
                _serviceProvider.GetRequiredService<
                    IWingsMemberAssessmentCommand>();

            var result = await handler.Handle(
                new WingsMemberAssessmentCommand
                {
                    Id = Guid.NewGuid()
                },
                CancellationToken.None);

            Assert.Equal("Success Processed 1 Gold member row(s) for Wings assessment.", result);
        }

        [Fact]
        public async Task Handle_ShouldMapMissingWingsAwardAsInactive()
        {
            ConfigureSingleMember(mockScenario: "missing");

            var handler =
                _serviceProvider.GetRequiredService<
                    IWingsMemberAssessmentCommand>();

            var result = await handler.Handle(
                new WingsMemberAssessmentCommand
                {
                    Id = Guid.NewGuid()
                },
                CancellationToken.None);

            Assert.Equal("Success Processed 1 Gold member row(s) for Wings assessment.", result);
        }

        [Fact]
        public async Task Handle_ShouldMapFutureDatedWingsAwardAsInactive()
        {
            ConfigureSingleMember(
                mockScenario: "future");

            var handler =
                _serviceProvider.GetRequiredService<
                    IWingsMemberAssessmentCommand>();

            var result = await handler.Handle(
                new WingsMemberAssessmentCommand
                {
                    Id = Guid.NewGuid()
                },
                CancellationToken.None);

            Assert.Equal("Success Processed 1 Gold member row(s) for Wings assessment.", result);
        }

        [Fact]
        public async Task Handle_ShouldFailWhenRulesEngineReportsRuntimeErrors()
        {
            ConfigureSingleMember(
                mockScenario: "active",
                rulesEngineMockScenario: "runtime-errors");

            var exception =
                await ExecuteSingleMemberAssessmentAsync();

            Assert.Equal(
                "Rules Engine returned an invalid Wings assessment response.",
                exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldFailWhenRulesEngineResponseEntityIsMissing()
        {
            ConfigureSingleMember(
                mockScenario: "active",
                rulesEngineMockScenario: "missing-response");

            var exception =
                await ExecuteSingleMemberAssessmentAsync();

            Assert.Equal(
                "Rules Engine returned an invalid Wings assessment response.",
                exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldFailWhenRulesEngineResponseBelongsToAnotherMember()
        {
            ConfigureSingleMember(
                mockScenario: "active",
                rulesEngineMockScenario: "another-customer");

            var exception =
                await ExecuteSingleMemberAssessmentAsync();

            Assert.Equal(
                "Rules Engine response does not match the member being assessed.",
                exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldFailWhenRulesEngineActionsAreEmpty()
        {
            ConfigureSingleMember(
                mockScenario: "active",
                rulesEngineMockScenario: "empty-actions");

            var exception =
                await ExecuteSingleMemberAssessmentAsync();

            Assert.Equal(
                "Rules Engine returned invalid Wings assessment actions.",
                exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldFailWhenRulesEngineActionIsInvalid()
        {
            ConfigureSingleMember(
                mockScenario: "active",
                rulesEngineMockScenario: "invalid-action");

            var exception =
                await ExecuteSingleMemberAssessmentAsync();

            Assert.Equal(
                "Rules Engine returned invalid Wings assessment actions.",
                exception.Message);
        }

        [Fact]
        public void WingsMemberAssessmentInRuleResponse_ShouldExposeReturnedActions()
        {
            var response =
                new WingsMemberAssessmentInRuleResponse
                {
                    ResponseEntity =
                        new WingsMemberAssessmentActionsResponse
                        {
                            CustomerId = "customer-id",
                            RuleResponse =
                                new WingsMemberAssessmentRuleResponse
                                {
                                    Actions =
                                        new List<WingsMemberAssessmentAction>
                                        {
                                            new()
                                            {
                                                Name = "WingsAction",
                                                Value = "Create"
                                            }
                                        }
                                }
                        }
                };

            var action =
                Assert.Single(
                    response.ResponseEntity.RuleResponse.Actions);

            Assert.Equal(
                "customer-id",
                response.ResponseEntity.CustomerId);

            Assert.Equal(
                "WingsAction",
                action.Name);

            Assert.Equal(
                "Create",
                action.Value);
        }

        private AwardDocumentServiceQueryHandler
            GetAwardDocumentQueryHandler() =>
                _serviceProvider.GetRequiredService<
                    AwardDocumentServiceQueryHandler>();

        private FlyingClubMemberDocumentServiceQueryHandler
            GetFlyingClubMemberDocumentQueryHandler() =>
                _serviceProvider.GetRequiredService<
                    FlyingClubMemberDocumentServiceQueryHandler>();

        private CustomerTransactionDocumentsServiceQueryHandler
            GetCustomerTransactionDocumentsQueryHandler() =>
                _serviceProvider.GetRequiredService<
                    CustomerTransactionDocumentsServiceQueryHandler>();

        private IRulesEngineService GetRulesEngineService() =>
            _serviceProvider.GetRequiredService<IRulesEngineService>();

        private ILogger<WingsMemberAssessmentCommandHandler> GetLogger() =>
            _serviceProvider.GetRequiredService<
                ILogger<WingsMemberAssessmentCommandHandler>>();

        private async Task<InvalidOperationException>
            ExecuteSingleMemberAssessmentAsync()
        {
            var handler =
                _serviceProvider.GetRequiredService<
                    IWingsMemberAssessmentCommand>();

            return await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(
                    new WingsMemberAssessmentCommand
                    {
                        Id = Guid.NewGuid()
                    },
                    CancellationToken.None));
        }

        private static void SeedLegacyCustomerTransactionAccessToken()
        {
            var cacheField = typeof(AzureADService).GetField(
                "cache",
                BindingFlags.Static | BindingFlags.NonPublic);
            var cache = Assert.IsType<ConcurrentDictionary<string, string>>(
                cacheField?.GetValue(null));
            var token = CreateValidAccessToken();

            cache["wings-test-client-id/wings-active"] = token;
        }

        private static string CreateValidAccessToken()
        {
            var expiresInMilliseconds = TimeSpan.FromHours(1).TotalMilliseconds;
            var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{}"))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                    $"{{\"created_at\":\"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}\",\"expires_in\":\"{expiresInMilliseconds}\"}}"))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            return $"{header}.{payload}.signature";
        }

        private void ConfigureSingleMember(
            string mockScenario,
            string? rulesEngineMockScenario = null)
        {
            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.WingsAssessmentBatchSize,
                "100");

            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.WingsAssessmentProductId,
                "1634561747");

            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.MillionMilerProductId,
                "1634561592");

            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.CustomerTxnMSDetailsEndpoint,
                "https://tempuri.org/customer/{0}/documents/{1}");

            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.CustomerTxnMSResourceId,
                $"wings-{mockScenario}");

            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.LoyaltyMbrMgmtOcpKey,
                "test-subscription-key");

            var resolvedRulesEngineMockScenario =
                rulesEngineMockScenario ?? $"{mockScenario}-request";

            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.RuleEngineUrl,
                $"https://tempuri.org/inrule/{resolvedRulesEngineMockScenario}");

            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.RuleEngineAppId,
                "wings-active");

            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.RuleEngineOcpSubscriptionId,
                "wings-rules-subscription-key");

            var reportingDbServices =
                _serviceProvider.GetRequiredService<MockReportingDbServices>();

            reportingDbServices.MemberBatch =
                new WingsAssessmentMemberBatch(
                    new[]
                    {
                        new GoldMemberDetails
                        {
                            CustomerId = "customer123",
                            LoyaltyMemberNumber = "member-1"
                        }
                    },
                    false);

        }

    }
}
