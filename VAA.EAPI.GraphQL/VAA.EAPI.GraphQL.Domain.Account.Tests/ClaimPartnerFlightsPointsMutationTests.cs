using HotChocolate;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Retro;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.CreateTransaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.SkyMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class ClaimPartnerFlightsPointsMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ISkyRetroProvider<CheckEligibilityClaimResponse> _skyProviderEligibility;
        private readonly ISkyRetroProvider<SkyFeedbackResponse> _skyProviderFeedback;
        private readonly IRefDataService<ProductDetails> _productRefService;
        private readonly IRefDataService<PartnerProfileDetails> _partnerProfileService;
        private readonly IRefDataService<AirportInfo> _airportInfoService;
        private readonly IRefDataService<AirlineInfo> _airlineInfoService;
        private readonly IRulesEngineProvider<AccrualPointsCalculatorResponse> _ruleEngineProvider;
        private readonly ILoyaltyMemberManagementProvider _loyaltyMemberManagementProvider;
        private readonly ITransactionMutationProvider _transactionMutationProvider;
        private readonly ICustomerTransactionQueryProvider<FlightTransactionResponse> _customerTransactionQueryProvider;
        //private Task<MissingPointsResponse<ClaimFlightSegmentDetails>> result;
        private readonly ICustomerTransactionQueryProvider<TransactionResponse<Transaction>> _customerTransactionDocProvider;
        private readonly IOAuthTokenProvider _tokenProvider;

        private readonly ClaimPartnerFlightsPointsMutation _mutation;
        private readonly ILogger<HttpConnector> _logger;
        private readonly HttpConnector _httpConnector;
        private readonly ILogger<ClaimPartnerFlightsPointsMutation> _claimLogger;

        public ClaimPartnerFlightsPointsMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _tokenProvider = new TestOAuthTokenProvider();
            var customerService = new CustomerService(_tokenProvider, _httpConnector);
            _tokenB2CProvider = new TokenB2CProvider<string>(_tokenProvider, _httpConnector, customerService, Microsoft.Extensions.Logging.Abstractions.NullLogger<TokenB2CProvider<string>>.Instance);
            _claimLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ClaimPartnerFlightsPointsMutation>.Instance;
            _customerProvider = customerService;
            _skyProviderEligibility = new SkyRetroProvider<CheckEligibilityClaimResponse>(_tokenProvider, _httpConnector);
            _skyProviderFeedback = new SkyRetroProvider<SkyFeedbackResponse>(_tokenProvider, _httpConnector);
            _productRefService = new RefDataService<ProductDetails>(_httpConnector);
            _partnerProfileService = new RefDataService<PartnerProfileDetails>(_httpConnector);
            _airportInfoService = new RefDataService<AirportInfo>(_httpConnector);
            _airlineInfoService = new RefDataService<AirlineInfo>(_httpConnector);
            _ruleEngineProvider = new RulesEngineProvider<AccrualPointsCalculatorResponse>(_tokenProvider, _httpConnector);
            _loyaltyMemberManagementProvider = new LoyaltyMemberManagementProvider(_tokenProvider, _httpConnector);
            _transactionMutationProvider = new TransactionMutationProvider(_tokenProvider, _httpConnector, _productRefService, _partnerProfileService);
            _customerTransactionQueryProvider = new CustomerTransactionQueryProvider<FlightTransactionResponse>(_tokenProvider, _httpConnector);
            _customerTransactionDocProvider = new CustomerTransactionQueryProvider<TransactionResponse<Transaction>>(_tokenProvider, _httpConnector);
            _mutation = new ClaimPartnerFlightsPointsMutation(_claimLogger, _tokenB2CProvider, _httpConnector, _customerProvider, _skyProviderEligibility, _skyProviderFeedback, _productRefService, _partnerProfileService, _airportInfoService, _airlineInfoService, _ruleEngineProvider, _loyaltyMemberManagementProvider, _transactionMutationProvider, _customerTransactionQueryProvider, _customerTransactionDocProvider);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenloggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(null, _tokenB2CProvider, _httpConnector, _customerProvider, _skyProviderEligibility, _skyProviderFeedback, _productRefService, _partnerProfileService, _airportInfoService, _airlineInfoService, _ruleEngineProvider, _loyaltyMemberManagementProvider, _transactionMutationProvider, _customerTransactionQueryProvider, _customerTransactionDocProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenB2CProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(_claimLogger, null, _httpConnector, _customerProvider, _skyProviderEligibility, _skyProviderFeedback, _productRefService, _partnerProfileService, _airportInfoService, _airlineInfoService, _ruleEngineProvider, _loyaltyMemberManagementProvider, _transactionMutationProvider, _customerTransactionQueryProvider, _customerTransactionDocProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenHttpConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(_claimLogger, _tokenB2CProvider, null, _customerProvider, _skyProviderEligibility, _skyProviderFeedback, _productRefService, _partnerProfileService, _airportInfoService, _airlineInfoService, _ruleEngineProvider, _loyaltyMemberManagementProvider, _transactionMutationProvider, _customerTransactionQueryProvider, _customerTransactionDocProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(_claimLogger, _tokenB2CProvider, _httpConnector, null, _skyProviderEligibility, _skyProviderFeedback, _productRefService, _partnerProfileService, _airportInfoService, _airlineInfoService, _ruleEngineProvider, _loyaltyMemberManagementProvider, _transactionMutationProvider, _customerTransactionQueryProvider, _customerTransactionDocProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenSkyProviderEligibilityObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(_claimLogger, _tokenB2CProvider, _httpConnector, _customerProvider, null, _skyProviderFeedback, _productRefService, _partnerProfileService, _airportInfoService, _airlineInfoService, _ruleEngineProvider, _loyaltyMemberManagementProvider, _transactionMutationProvider, _customerTransactionQueryProvider, _customerTransactionDocProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenSkyProviderFeedBackObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(_claimLogger, _tokenB2CProvider, _httpConnector, _customerProvider, _skyProviderEligibility, null, _productRefService, _partnerProfileService, _airportInfoService, _airlineInfoService, _ruleEngineProvider, _loyaltyMemberManagementProvider, _transactionMutationProvider, _customerTransactionQueryProvider, _customerTransactionDocProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenProductRefServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(_claimLogger, _tokenB2CProvider, _httpConnector, _customerProvider, _skyProviderEligibility, _skyProviderFeedback, null, _partnerProfileService, _airportInfoService, _airlineInfoService, _ruleEngineProvider, _loyaltyMemberManagementProvider, _transactionMutationProvider, _customerTransactionQueryProvider, _customerTransactionDocProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenPartnerProfileServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(_claimLogger, _tokenB2CProvider, _httpConnector, _customerProvider, _skyProviderEligibility, _skyProviderFeedback, _productRefService, null, _airportInfoService, _airlineInfoService, _ruleEngineProvider, _loyaltyMemberManagementProvider, _transactionMutationProvider, _customerTransactionQueryProvider, _customerTransactionDocProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenAirportInfoServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(_claimLogger, _tokenB2CProvider, _httpConnector, _customerProvider, _skyProviderEligibility, _skyProviderFeedback, _productRefService, _partnerProfileService, null, _airlineInfoService, _ruleEngineProvider, _loyaltyMemberManagementProvider, _transactionMutationProvider, _customerTransactionQueryProvider, _customerTransactionDocProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenRuleEnginePRoviderProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(_claimLogger, _tokenB2CProvider, _httpConnector, _customerProvider, _skyProviderEligibility, _skyProviderFeedback, _productRefService, _partnerProfileService, _airportInfoService, _airlineInfoService, null, _loyaltyMemberManagementProvider, _transactionMutationProvider, _customerTransactionQueryProvider, _customerTransactionDocProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoyaltyMemberManagementProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(_claimLogger, _tokenB2CProvider, _httpConnector, _customerProvider, _skyProviderEligibility, _skyProviderFeedback, _productRefService, _partnerProfileService, _airportInfoService, _airlineInfoService, _ruleEngineProvider, null, _transactionMutationProvider, _customerTransactionQueryProvider, _customerTransactionDocProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTransactionMutationProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(_claimLogger, _tokenB2CProvider, _httpConnector, _customerProvider, _skyProviderEligibility, _skyProviderFeedback, _productRefService, _partnerProfileService, _airportInfoService, _airlineInfoService, _ruleEngineProvider, _loyaltyMemberManagementProvider, null, _customerTransactionQueryProvider, _customerTransactionDocProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerTransactionQueryProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ClaimPartnerFlightsPointsMutation(_claimLogger, _tokenB2CProvider, _httpConnector, _customerProvider, _skyProviderEligibility, _skyProviderFeedback, _productRefService, _partnerProfileService, _airportInfoService, _airlineInfoService, _ruleEngineProvider, _loyaltyMemberManagementProvider, _transactionMutationProvider, null, _customerTransactionDocProvider));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullLoginToken()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SkyFlightsPointsClaimRequest>("Account//ClaimPartnerFlightsPointsMutationRequest");

            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _mutation.ClaimMissingPointsSky(request));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullTicketNumber()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SkyFlightsPointsClaimRequest>("Account//ClaimPartnerFlightsPointsMutationRequest");
            request.TicketNumber = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutation.ClaimMissingPointsSky(request));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullFcNumber()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SkyFlightsPointsClaimRequest>("Account//ClaimPartnerFlightsPointsMutationRequest");
            request.FcNumber = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutation.ClaimMissingPointsSky(request));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullDepartureDate()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SkyFlightsPointsClaimRequest>("Account//ClaimPartnerFlightsPointsMutationRequest");
            request.DepartureDate = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutation.ClaimMissingPointsSky(request));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullOriginAirportCode()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SkyFlightsPointsClaimRequest>("Account//ClaimPartnerFlightsPointsMutationRequest");
            request.DepartureFrom = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutation.ClaimMissingPointsSky(request));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullDestinationAirportCode()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SkyFlightsPointsClaimRequest>("Account//ClaimPartnerFlightsPointsMutationRequest");
            request.ArrivingAt = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutation.ClaimMissingPointsSky(request));
        }

        [Fact]
        public async Task Mutation_ShouldReturnSegmentandPointResponse_ForGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SkyFlightsPointsClaimRequest>("Account//ClaimPartnerFlightsPointsMutationRequest");
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

            //Act
            //Configure TestContext first then set to current AsyncContext
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", loginToken }
            };

            var testContext = new TestFunctionContext(bindingData);

            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));

            AsyncContext.Current = new AsyncContext() { Context = testContext };

            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/Good");

            var result = await _mutation.ClaimMissingPointsSky(request);

            //Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorResponse);
            Assert.IsType<MissingPointsResponse<ClaimFlightSegmentDetails>>(result);
            Assert.Equal("1382", result.TotalBonusPoints);
            Assert.Equal("200", result.TotalTierPoints);
            Assert.IsType<ClaimFlightSegmentDetails>(result.segmentDetails.First());
            Assert.Equal("AF", result.segmentDetails.First().OperatingAirlineCode);
            Assert.Equal("LHR", result.segmentDetails.First().OriginAirportCode);
            Assert.Equal("Indira Gandhi International Airport", result.segmentDetails.First().DestinationAirportName);
            Assert.Equal("1638", result.segmentDetails.First().VirginPoints);
            Assert.Equal("AF612", result.segmentDetails.First().FlightNumber);
        }

        [Fact]
        public async Task Mutation_ShouldThrowError_AfterFailingToValidateToken()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SkyFlightsPointsClaimRequest>("Account//ClaimPartnerFlightsPointsMutationRequest");
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

            //Act

            // Configure TestContext first then set to current AsyncContext
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", loginToken }
            };

            var testContext = new TestFunctionContext(bindingData);

            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));

            AsyncContext.Current = new AsyncContext() { Context = testContext };

            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/Bad");

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.ClaimMissingPointsSky(request));
        }

        [Fact]
        public async Task Constructor_ShouldThrowException_WhenCustomerTransactionDupilicateCheckConfirmsExistingTransaction()
        {
            ConfigReader.Set(Common.Constants.Environment.CustTransactionMsUrl, "https://testresources.virginatlantic.com/dev/customertransactions/true");

            //Arrange
            var request = TestDataProvider.GetTestData<SkyFlightsPointsClaimRequest>("Account//ClaimPartnerFlightsPointsMutationRequest");
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

            //Act

            // Configure TestContext first then set to current AsyncContext
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", loginToken }
            };

            var testContext = new TestFunctionContext(bindingData);

            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));

            AsyncContext.Current = new AsyncContext() { Context = testContext };
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/Good");

            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.ClaimMissingPointsSky(request));
        }

        [Fact]
        public async Task Mutation_ShouldThrowError_ForInvalidAirlineCode()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SkyFlightsPointsClaimRequest>("Account//ClaimPartnerFlightsPointsMutationRequest");
            request.Airline = "test123";
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

            //Act

            // Configure TestContext first then set to current AsyncContext
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", loginToken }
            };

            var testContext = new TestFunctionContext(bindingData);

            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));

            AsyncContext.Current = new AsyncContext() { Context = testContext };
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/Bad");

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.ClaimMissingPointsSky(request));
        }

        [Fact]
        public async Task Mutation_ShouldThrowError_ForMissingProductCode()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SkyFlightsPointsClaimRequest>("Account//ClaimPartnerFlightsPointsMutationRequest");
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

            //Act

            // Configure TestContext first then set to current AsyncContext
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", loginToken }
            };

            var testContext = new TestFunctionContext(bindingData);

            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));

            AsyncContext.Current = new AsyncContext() { Context = testContext };
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/Bad");

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.ClaimMissingPointsSky(request));
        }

        [Fact]
        public async Task Mutation_ShouldThrowError_ForFailedVredTransactionCreation()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<SkyFlightsPointsClaimRequest>("Account//ClaimPartnerFlightsPointsMutationRequest");
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

            //Act

            // Configure TestContext first then set to current AsyncContext
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", loginToken }
            };

            var testContext = new TestFunctionContext(bindingData);

            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));

            AsyncContext.Current = new AsyncContext() { Context = testContext };
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/Good");

            ConfigReader.Set(Common.Constants.Environment.VredMsUrl, "https://Testresources.virginatlantic.com/dev/vred/VredTransaction/createTransaction/Bad");

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.ClaimMissingPointsSky(request));
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("dummy-token");
        }
    }
}
