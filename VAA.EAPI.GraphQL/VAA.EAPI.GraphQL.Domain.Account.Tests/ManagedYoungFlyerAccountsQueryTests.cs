using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class ManagedYoungFlyerAccountsQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private const string IdentityUrl = "https://Testresources.virginatlantic.com/dev/identitymicroservice/Good";
        private const string CustomerUrl = "https://testresources.virginatlantic.com/youngflyer/customer/{customerid}";
        private const string MemberManagementUrl = "https://testresources.virginatlantic.com/youngflyer/membermanagement";
        private const string CustomerTransactionsUrl = "https://testresources.virginatlantic.com/youngflyer/customertransactions/";
        private const string VredUrl = "https://testresources.virginatlantic.com/youngflyer/vred";
        private const string RefDataUrl = "https://testresources.virginatlantic.com/youngflyer/reference/{0}";
        private const int ActivitiesCount = 50;
        private readonly HttpClientFactoryFixture _fixture;
        private readonly IServiceProvider _serviceProvider;

        public ManagedYoungFlyerAccountsQueryTests(HttpClientFactoryFixture fixture)
        {
            _fixture = fixture;
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.IdentityMsUrl, IdentityUrl);
            ConfigReader.Set(Environment.CustomerUrlGetbyCustID, CustomerUrl);
            ConfigReader.Set(Environment.LoyaltyMemberManagementMsUrl, MemberManagementUrl);
            ConfigReader.Set(Environment.CustTransactionMsUrl, CustomerTransactionsUrl);
            ConfigReader.Set(Environment.VredMsUrl, VredUrl);
            ConfigReader.Set(Environment.RefDataMSGenericUrl, RefDataUrl);

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(fixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider, TestOAuthTokenProvider>()
                .AddSingleton<IAccountCustomerService, CustomerService>()
                .AddSingleton<ICustomerTransactionQueryProvider<FcMemberDetails>, CustomerTransactionQueryProvider<FcMemberDetails>>()
                .AddSingleton<ICustomerTransactionQueryProvider<CustomerTransactionDocument>, CustomerTransactionQueryProvider<CustomerTransactionDocument>>()
                .AddSingleton<ICustomerTransactionQueryProvider<TransactionResponse<TransactionDoc>>, CustomerTransactionQueryProvider<TransactionResponse<TransactionDoc>>>()
.AddSingleton(typeof(IRefDataService<>), typeof(VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Services.RefDataService<>))
                .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
                .AddSingleton<IMemberCustomAttributeProvider, MemberCustomAttributeProvider>()
                .AddSingleton<IActivityQueryProvider, ActivityQueryProvider>()
                .AddSingleton<IManagedYoungFlyerRelationshipProvider, ManagedYoungFlyerRelationshipProvider>()
                .AddSingleton<IManagedYoungFlyerAccountProvider, ManagedYoungFlyerAccountProvider>()
                .AddSingleton<ManagedYoungFlyerAccountsQuery>()
                .AddSingleton<TimeProvider>(new FixedTimeProvider(new DateTimeOffset(2026, 10, 14, 0, 0, 0, TimeSpan.Zero)))
                .BuildServiceProvider();
        }

        [Fact]
        public async Task GetManagedYoungFlyerAccounts_ReturnsManagedYoungFlyersFromAuthenticatedParent()
        {
            SetAuthenticatedToken();

            var result = await _serviceProvider.GetRequiredService<ManagedYoungFlyerAccountsQuery>().GetManagedYoungFlyerAccounts(ActivitiesCount);

            var child = Assert.Single(result.YoungFlyers);
            Assert.Equal("child-id", child.CustomerId);
            Assert.Equal("CHILD", child.FcNumber);
            Assert.True(child.HoHManaged);
            Assert.Equal("child@example.com", child.EmailAddress);
            Assert.Equal(DateTimeKind.Utc, child.DateOfBirth?.Kind);
            Assert.Equal("2010-01-01T00:00:00.000Z", child.DateOfBirth?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
            Assert.Equal(400, child.TierPoints);
            Assert.Equal("2020-01-02T00:00:00.000Z", child.FCJoinDate?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
            Assert.Equal("Silver", child.TierCode);
            Assert.Equal(600, child.TierPointsNeedToReachNextTier);
            Assert.Equal(300, child.RetainTierPoints);
            Assert.Equal(500, child.TierPointsNeedToRetain);
            Assert.Equal(700, child.RollingTierPoints);
        }

        [Fact]
        public async Task GetManagedYoungFlyerAccounts_StopsBeforeDownstreamCalls_WhenRequestIsCancelled()
        {
            SetAuthenticatedToken();
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                _serviceProvider.GetRequiredService<ManagedYoungFlyerAccountsQuery>().GetManagedYoungFlyerAccounts(ActivitiesCount, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task GetManagedYoungFlyerAccounts_RegistersAndExecutesNestedEnrichmentFieldsInSchema()
        {
            SetAuthenticatedToken();
            var queryResult = await ExecuteGraphQlQuery("""
                query {
                  accountManagedYoungFlyerAccounts(activitiesCount: 50) {
                    youngFlyers {
                      activityResponse { activityDetails { transactionId } errorResponse { message } }
                      tenureResponse { memberCustomAttributesDocuments { totalActiveYear } errorResponse { message } }
                    }
                  }
                }
                """);

            Assert.Null(queryResult.Errors);
            Assert.NotNull(queryResult.Data);
            var response = queryResult.ToJson();
            Assert.Contains("child-activity-transaction", response);
            Assert.Equal("2", JObject.Parse(response).SelectToken("data.accountManagedYoungFlyerAccounts.youngFlyers[0].tenureResponse.memberCustomAttributesDocuments[0].totalActiveYear")?.Value<string>());
        }

        [Fact]
        public async Task GetManagedYoungFlyerAccounts_ReturnsActivityFallback_WhenChildActivityRetrievalFails()
        {
            ConfigReader.Set(Environment.VredMsUrl, "https://testresources.virginatlantic.com/youngflyer/vred-failure");
            SetAuthenticatedToken();

            var result = await ExecuteGraphQlQuery("""
                query {
                  accountManagedYoungFlyerAccounts(activitiesCount: 50) {
                    youngFlyers {
                      activityResponse { activityDetails { transactionId } errorResponse { message extentions { exception code } } }
                    }
                  }
                }
                """);

            Assert.Null(result.Errors);
            Assert.Contains("Unable to retrieve Activities", result.ToJson());
            Assert.Contains("G053", result.ToJson());
            Assert.Empty(JObject.Parse(result.ToJson()).SelectToken("data.accountManagedYoungFlyerAccounts.youngFlyers[0].activityResponse.activityDetails")!.Values());
        }

        [Fact]
        public async Task GetManagedYoungFlyerAccounts_ReturnsTenureFallback_WhenChildTenureRetrievalFails()
        {
            ConfigReader.Set(Environment.CustTransactionMsUrl, "https://testresources.virginatlantic.com/youngflyer/customertransactions-tenure-failure/");
            SetAuthenticatedToken();

            var result = await ExecuteGraphQlQuery("""
                query {
                  accountManagedYoungFlyerAccounts(activitiesCount: 50) {
                    youngFlyers {
                      tenureResponse { memberCustomAttributesDocuments { totalActiveYear } errorResponse { message extentions { exception code } } }
                    }
                  }
                }
                """);

            Assert.Null(result.Errors);
            Assert.Contains("Unable to retrieve High Five details", result.ToJson());
            Assert.Contains("G067", result.ToJson());
        }

        [Fact]
        public async Task GetManagedYoungFlyerAccounts_RejectsMissingAuthenticationContext()
        {
            AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };

            await Assert.ThrowsAsync<ArgumentNullException>(() => _serviceProvider.GetRequiredService<ManagedYoungFlyerAccountsQuery>().GetManagedYoungFlyerAccounts(ActivitiesCount));
        }

        [Fact]
        public async Task GetManagedYoungFlyerAccounts_ReturnsEmptyList_WhenParentHasNoManagedYoungFlyers()
        {
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/youngflyer/identity/EmptyParent");
            SetAuthenticatedToken();

            var result = await _serviceProvider.GetRequiredService<ManagedYoungFlyerAccountsQuery>().GetManagedYoungFlyerAccounts(ActivitiesCount);

            Assert.Empty(result.YoungFlyers);
        }

        [Fact]
        public async Task GetManagedYoungFlyerAccounts_ExcludesYoungFlyer_WhenCustomerDocumentIsMissing()
        {
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/youngflyer/identity/NoCustomerParent");
            SetAuthenticatedToken();

            var result = await _serviceProvider.GetRequiredService<ManagedYoungFlyerAccountsQuery>().GetManagedYoungFlyerAccounts(ActivitiesCount);

            Assert.Empty(result.YoungFlyers);
        }

        [Fact]
        public async Task GetManagedYoungFlyerAccounts_ExcludesYoungFlyer_WhenCustomerProfileRetrievalFails()
        {
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/youngflyer/identity/PartialParent");
            SetAuthenticatedToken();

            var result = await _serviceProvider.GetRequiredService<ManagedYoungFlyerAccountsQuery>().GetManagedYoungFlyerAccounts(ActivitiesCount);

            var youngFlyers = result.YoungFlyers.ToList();
            Assert.Single(youngFlyers);

            var child = Assert.Single(youngFlyers);
            Assert.Equal("child-id", child.CustomerId);
            Assert.True(child.HoHManaged);
        }

        [Fact]
        public async Task GetManagedYoungFlyerAccounts_ReturnsDownstreamErrorDetails_WhenIdentityValidationFails()
        {
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/youngflyer/identity/ErrorParent");
            SetAuthenticatedToken();

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _serviceProvider.GetRequiredService<ManagedYoungFlyerAccountsQuery>().GetManagedYoungFlyerAccounts(ActivitiesCount));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("IDENTITY-001", error.Code);
            Assert.Equal("Token validation failed", error.Message);
            Assert.Equal("Identity MS", error.Extensions!["source"]);
        }

        [Fact]
        public async Task GetManagedYoungFlyerAccounts_ReturnsTechnicalErrorDetails_WhenIdentityResponseIsMalformed()
        {
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/youngflyer/identity/MalformedParent");
            SetAuthenticatedToken();

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _serviceProvider.GetRequiredService<ManagedYoungFlyerAccountsQuery>().GetManagedYoungFlyerAccounts(ActivitiesCount));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("Miscellaneous technical error", error.Message);
            Assert.Equal("G063", error.Code);
            Assert.True(error.Extensions!.ContainsKey("Exception"));
        }

        [Fact]
        public async Task GetManagedYoungFlyerAccounts_ReturnsDownstreamErrorDetails_WhenLoyaltyMemberManagementFails()
        {
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/youngflyer/identity/LoyaltyErrorParent");
            SetAuthenticatedToken();

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _serviceProvider.GetRequiredService<ManagedYoungFlyerAccountsQuery>().GetManagedYoungFlyerAccounts(ActivitiesCount));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("LMM-001", error.Code);
            Assert.Equal("Household details unavailable", error.Message);
            Assert.Equal("Loyalty Member Management MS", error.Extensions!["source"]);
        }


        private static void SetAuthenticatedToken(string token = "jwt-token")
        {
            AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
            AsyncContext.Current.Context.Features.Set(new JwtPrincipalFeature(new ClaimsPrincipal(), new AuthToken { Token = token }));
        }

        private async Task<IQueryResult> ExecuteGraphQlQuery(string query)
        {
            var services = new ServiceCollection()
                .AddSingleton(_serviceProvider.GetRequiredService<ManagedYoungFlyerAccountsQuery>())
                .AddSingleton(_serviceProvider.GetRequiredService<IActivityQueryProvider>())
                .AddSingleton(_serviceProvider.GetRequiredService<IMemberCustomAttributeProvider>())
                .AddSingleton(_serviceProvider.GetRequiredService<TimeProvider>())
                .AddSingleton(_serviceProvider.GetRequiredService<ILogger<ManagedYoungFlyerAccountsQueryResolver>>())
                .AddLogging();

            services.AddGraphQLServer()
                .AddQueryType(descriptor => descriptor.Name(OperationTypeNames.Query))
                .AddTypeExtension<ManagedYoungFlyerAccountsQuery>()
                .AddType<ManagedYoungFlyerAccountsQueryResolver>();

            await using var schemaProvider = services.BuildServiceProvider();
            var executor = await schemaProvider.GetRequestExecutorAsync();
            return Assert.IsAssignableFrom<IQueryResult>(await executor.ExecuteAsync(query));
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("test-token");
        }

        private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => utcNow;
        }
    }
}
