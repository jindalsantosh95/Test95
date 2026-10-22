using HotChocolate;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class MemberDetailsQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly MemberDetailsQuery _query;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IRefDataService<TierThresholdDetails> _tierThresholdService;
        private readonly ICustomerTransactionQueryProvider<CustomerProfileDTO> _customerTransCustomerProfile;
        private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _customerTransFlyingClub;
        private readonly IRefDataService<CountryRefData> _countryRefService;
        private readonly ILogger<MemberDetailsQuery> _queryLogger;

        public MemberDetailsQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigureBaseEnvironment();

            _queryLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<MemberDetailsQuery>.Instance;
            var httpLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _tokenProvider = new TestOAuthTokenProvider();
            _connector = new HttpConnector(httpLogger, httpClientFactoryFixture.Current);

            var customerService = new CustomerService(_tokenProvider, _connector);
            _tokenB2CProvider = new TokenB2CProvider<string>(_tokenProvider, _connector, customerService, Microsoft.Extensions.Logging.Abstractions.NullLogger<TokenB2CProvider<string>>.Instance);
            _tierThresholdService = new RefDataService<TierThresholdDetails>(_connector);
            _customerTransCustomerProfile = new CustomerTransactionQueryProvider<CustomerProfileDTO>(_tokenProvider, _connector);
            _customerTransFlyingClub = new CustomerTransactionQueryProvider<FcMemberDetails>(_tokenProvider, _connector);
            _countryRefService = new RefDataService<CountryRefData>(_connector);
            _query = new MemberDetailsQuery(_queryLogger, _tokenProvider, _connector, _tokenB2CProvider, _tierThresholdService, _customerTransCustomerProfile, _customerTransFlyingClub, _countryRefService);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberDetailsQuery(null, _tokenProvider, _connector, _tokenB2CProvider, _tierThresholdService, _customerTransCustomerProfile, _customerTransFlyingClub, _countryRefService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhentokenProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberDetailsQuery(_queryLogger, null, _connector, _tokenB2CProvider, _tierThresholdService, _customerTransCustomerProfile, _customerTransFlyingClub, _countryRefService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberDetailsQuery(_queryLogger, _tokenProvider, null, _tokenB2CProvider, _tierThresholdService, _customerTransCustomerProfile, _customerTransFlyingClub, _countryRefService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenb2cIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberDetailsQuery(_queryLogger, _tokenProvider, _connector, null, _tierThresholdService, _customerTransCustomerProfile, _customerTransFlyingClub, _countryRefService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTierThresholdServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberDetailsQuery(_queryLogger, _tokenProvider, _connector, _tokenB2CProvider, null, _customerTransCustomerProfile, _customerTransFlyingClub, _countryRefService));
        }

        [Fact]
        public async Task Query_ShouldThrowException_ForNullLoginToken()
        {
            SetRequestContext(null);

            await Assert.ThrowsAsync<ArgumentNullException>(() => _query.GetMemberDetails(1, 3, 1));
        }

        [Fact]
        public async Task Query_ShouldThorwException_AfterFailingtoConnecttoIdentityMS()
        {
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            SetRequestContext(loginToken);
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/memberdetailsbad");

            await Assert.ThrowsAsync<GraphQLException>(() => _query.GetMemberDetails(1, 3, 1));
        }

        [Fact]
        public async Task Query_ShouldReturnPersonalInfo_ForGoodRequest()
        {
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            ConfigureBaseEnvironment();
            SetRequestContext(loginToken);

            var result = await _query.GetMemberDetails(1, 3, 1);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("David", result.flyingClubMemberDetails.FirstName);
            Assert.Equal(9230232239, result.flyingClubMemberDetails.FCMembershipNumber);
            Assert.True(result.flyingClubMemberDetails.MemberStatus.IsWingsMemeber);
            Assert.True(result.flyingClubMemberDetails.MemberStatus.IsMillionMiler);
            Assert.True(result.flyingClubMemberDetails.MemberStatus.IsLifeTimeGoldMember);
            Assert.False(result.flyingClubMemberDetails.MemberStatus.Is2MillionMiler);
            Assert.Equal(250, result.flyingClubMemberDetails.RollingTierPoints);
            Assert.Equal(DateTime.Parse("2024-07-31T00:00:00.000+01:00"), result.flyingClubMemberDetails.UpgradeByDate);
            Assert.IsType<List<TierThresholdDetails>>(result.flyingClubMemberDetails.TierThresholdDetails);
            Assert.Contains(result.flyingClubMemberDetails.TierThresholdDetails, x => x.TierPointsForRetainToGold == 1000);
            Assert.Contains(result.flyingClubMemberDetails.TierThresholdDetails, x => x.TierPointsForRedToSilver == 400);
            Assert.Equal("44", result.flyingClubMemberDetails.CountryCode);
            Assert.Equal("Y", result.flyingClubMemberDetails.Address.First().PreferredFlag);
            Assert.Contains(result.flyingClubMemberDetails.Address, item => item.PostalCode == "E1 1AB");
            Assert.True(result.flyingClubMemberDetails.IsVAAMarketingPreferred);
            Assert.Equal("DL", result.flyingClubMemberDetails.PreferredPartnerFFP);
            Assert.Equal(HouseHoldMemberType.Household, result.flyingClubMemberDetails.MemberType);
            Assert.IsType<HouseHoldMemberType>(result.flyingClubMemberDetails.MemberType);
            Assert.Equal("GB", result.flyingClubMemberDetails.Address.First().CountryCode);
            Assert.Equal(DateTime.Parse("2022-05-09T14:30:45"), result.flyingClubMemberDetails.FCJoinDate);
            Assert.Equal(250, result.flyingClubMemberDetails.RevisedRollingTierPoints);
            Assert.Equal(124, result.flyingClubMemberDetails.RevisedRetainTierPoints);
        }

        [Fact]
        public async Task Query_ShouldThorwException_ForFailingtoConnecttoReferenceMS()
        {
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            ConfigureBaseEnvironment();
            ConfigReader.Set(Common.Constants.Environment.RefDataMSGenericUrl, "https://Testresources.virginatlantic.com/dev/ReferenceMicroservice/fail/{0}");
            SetRequestContext(loginToken);

            await Assert.ThrowsAsync<GraphQLException>(() => _query.GetMemberDetails(1, 3, 1));
        }

        [Fact]
        public async Task Query_ShouldReturnReturnVacOverviewResponse_ForGoodRequest()
        {
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            ConfigReader.Set(Common.Constants.Environment.CustomerMSURLForMemberDetails, "https://Testresources.virginatlantic.com/dev/microservices/memberdetailsvac/customer/loyalty/{memberNumber}");
            ConfigReader.Set(Common.Constants.Environment.CustTransactionMsUrl, "https://Testresources.virginatlantic.com/dev/customertransactions/memberdetailsvac/");
            SetRequestContext(loginToken);

            var result = await _query.GetMemberDetails(1, 3, 1);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("David", result.flyingClubMemberDetails.FirstName);
            Assert.Equal("VAC", result.flyingClubMemberDetails.TierCode);
            Assert.Equal("434km-345dq-45fw-454rf", result.flyingClubMemberDetails.CustomerID);
            Assert.Equal(default, result.flyingClubMemberDetails.FCMembershipNumber);
            Assert.Equal(default, result.flyingClubMemberDetails.RollingTierPoints);
            Assert.Equal(default, result.flyingClubMemberDetails.UpgradeByDate);
            Assert.Null(result.flyingClubMemberDetails.TierThresholdDetails);
            Assert.Equal("44", result.flyingClubMemberDetails.CountryCode);
            Assert.Null(result.flyingClubMemberDetails.Address);
            Assert.True(result.flyingClubMemberDetails.IsVAAMarketingPreferred);
            Assert.Equal("DL", result.flyingClubMemberDetails.PreferredPartnerFFP);
            Assert.Equal(HouseHoldMemberType.Individual, result.flyingClubMemberDetails.MemberType);
            Assert.IsType<HouseHoldMemberType>(result.flyingClubMemberDetails.MemberType);
        }

        [Theory]
        [InlineData("Red", false, 250, 124)]
        [InlineData("Silver", false, 250, 124)]
        [InlineData("Gold", false, 250, 124)]
        [InlineData("Gold", true, 250, 124)]
        public async Task Query_ShouldMapRevisedVsTierPoints_ByTier(string tierCode, bool isWingsMember, int expectedRolling, int expectedRetain)
        {
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            ConfigureBaseEnvironment();
            SetRequestContext(loginToken);

            var result = await _query.GetMemberDetails(1, 3, 1);

            result.flyingClubMemberDetails.TierCode = tierCode;
            result.flyingClubMemberDetails.MemberStatus.IsWingsMemeber = isWingsMember;
            result.flyingClubMemberDetails.ApplyRevisedVsTierPoints(250, 124);

            Assert.Equal(expectedRolling, result.flyingClubMemberDetails.RevisedRollingTierPoints);
            Assert.Equal(expectedRetain, result.flyingClubMemberDetails.RevisedRetainTierPoints);
        }

        private static void SetRequestContext(string token)
        {
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", token ?? string.Empty }
            };

            var testContext = new TestFunctionContext(bindingData);
            var requestBody = new StringContent("{}", Encoding.UTF8);
            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), requestBody.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), new Common.Entities.AuthToken { Token = token }));
            AsyncContext.Current = new AsyncContext { Context = testContext };
        }

        private static void ConfigureBaseEnvironment()
        {
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/memberdetailsgood");
            ConfigReader.Set(Common.Constants.Environment.CustomerMSURLForMemberDetails, "https://Testresources.virginatlantic.com/dev/microservices/memberdetails/customer/loyalty/{memberNumber}");
            ConfigReader.Set(Common.Constants.Environment.CustTransactionMsUrl, "https://Testresources.virginatlantic.com/dev/customertransactions/memberdetails/");
            ConfigReader.Set(Common.Constants.Environment.VredMsUrl, "https://Testresources.virginatlantic.com/dev/vred/memberdetails");
            ConfigReader.Set(Common.Constants.Environment.RefDataMSGenericUrl, "https://Testresources.virginatlantic.com/dev/ReferenceMicroservice/memberdetails/{0}");
            ConfigReader.Set(Common.Constants.Environment.LoyaltyMemberManagementMsUrl, "https://Testresources.virginatlantic.com/dev/loyaltymembermanagement/memberdetails");
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("abc");
        }
    }
}
