using HotChocolate;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class ManageAccountQueryTests
    {
        private readonly Mock<ITokenB2CProvider<string>> _tokenB2CProvider;
        private readonly Mock<IAccountCustomerService> _customerProvider;
        private readonly Mock<ILogger<ManageAccountQuery>> _mockLogger;
        private readonly Mock<ICustomerTransactionQueryProvider<CustomerProfileDTO>> _customerTransCustomerProfile;
        private readonly Mock<ICustomerTransactionQueryProvider<FcMemberDetails>> _customerTransFlyingClub;
        private readonly Mock<IRefDataService<CountryRefData>> _countryRefService;
        private readonly ManageAccountQuery _query;

        public ManageAccountQueryTests()
        {
            _tokenB2CProvider = new Mock<ITokenB2CProvider<string>>();
            _customerProvider = new Mock<IAccountCustomerService>();
            _mockLogger = new Mock<ILogger<ManageAccountQuery>>();
            _customerTransCustomerProfile = new Mock<ICustomerTransactionQueryProvider<CustomerProfileDTO>>();
            _customerTransFlyingClub = new Mock<ICustomerTransactionQueryProvider<FcMemberDetails>>();
            _countryRefService = new Mock<IRefDataService<CountryRefData>>();
            _query = new ManageAccountQuery(_tokenB2CProvider.Object, _customerProvider.Object, _mockLogger.Object, _customerTransCustomerProfile.Object, _customerTransFlyingClub.Object, _countryRefService.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenB2CProvideObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ManageAccountQuery(null, _customerProvider.Object, _mockLogger.Object, _customerTransCustomerProfile.Object, _customerTransFlyingClub.Object, _countryRefService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCcustomerProvideObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ManageAccountQuery(_tokenB2CProvider.Object, null, _mockLogger.Object, _customerTransCustomerProfile.Object, _customerTransFlyingClub.Object, _countryRefService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ManageAccountQuery(_tokenB2CProvider.Object, _customerProvider.Object, null, _customerTransCustomerProfile.Object, _customerTransFlyingClub.Object, _countryRefService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLogger_customerTransCustomerProfileIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ManageAccountQuery(_tokenB2CProvider.Object, _customerProvider.Object, _mockLogger.Object, null, _customerTransFlyingClub.Object, _countryRefService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerTransFlyingClubIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ManageAccountQuery(_tokenB2CProvider.Object, _customerProvider.Object, _mockLogger.Object, _customerTransCustomerProfile.Object, null, _countryRefService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCountryRefServiceNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ManageAccountQuery(_tokenB2CProvider.Object, _customerProvider.Object, _mockLogger.Object, _customerTransCustomerProfile.Object, _customerTransFlyingClub.Object, null));
        }

        [Fact]
        public async Task Query_ShouldThrowException_ForNullLoginToken()
        {
            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _query.ManageAccount());
        }

        [Fact]
        public async Task Query_ShouldReturnResponse_ForGoodRequest()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");
            var customerResponse = TestDataProvider.GetTestData<CustomerDTO>("Account//CustomerDTO");
            var customerProfileResponse = TestDataProvider.GetTestData<CustomerProfileDTO>("Account//CustomerProfileDocResponse");
            var customerFCMemberResponse = TestDataProvider.GetTestData<FcMemberDetails>("Account\\ProfileInfoResponse");
            var countriesRefResponse = TestDataProvider.GetTestData<IEnumerable<CountryRefData>>("Account//CountriesRefResponse");

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

            _tokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(authResponse);
            _customerProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);
            _customerTransCustomerProfile.Setup(c => c.CustomerTransactionQueryCustomerProfile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerProfileResponse);
            _customerTransFlyingClub.Setup(c => c.CustomerTransactionQuery(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerFCMemberResponse);
            _countryRefService.Setup(x => x.Query()).ReturnsAsync(countriesRefResponse);

            var result = await _query.ManageAccount();

            //Assert
            Assert.NotNull(result);
            Assert.Equal("CheckFirstNameTesting", result.FirstName);
            Assert.Equal("44", result.CountryCode);
            Assert.Equal("Y", result.Address.First().PreferredFlag);
            Assert.Contains(result.Address, item => item.PostalCode == "E1 1AB");
            Assert.IsType<ManageAccountResponse<AddressCustMS>>(result);
            Assert.True(result.IsVAAMarketingPreferred);
            Assert.Equal(HouseHoldMemberType.Household, result.MemberType);
            Assert.IsType<HouseHoldMemberType>(result.MemberType);
            Assert.Equal("GB", result.Address.First().CountryCode);
        }

        [Fact]
        public async Task Query_ShouldThorwException_ForFailingtoConnecttoCustomerMS()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");
            var authToken = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//AuthTokenFailedResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");

            var apiResponse = new HttpResponseMessage() { Content = authToken };

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

            _tokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(authResponse);

            _customerProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new HttpResponseException(apiResponse));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _query.ManageAccount());
        }

        [Fact]
        public async Task Query_ShouldThorwJsonSerializationException_ForExpiredorInvalidToken()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authToken = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//AuthTokenFailedResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");
            var apiResponse = new HttpResponseMessage() { Content = authToken };
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

            _tokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ThrowsAsync(new HttpResponseException(apiResponse));


            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _query.ManageAccount());
        }

        [Fact]
        public async Task Query_ShouldThorwException_ForFailingtoConnecttoIdentityMS()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authToken = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//AuthTokenFailedResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");
            var apiResponse = new HttpResponseMessage() { Content = authToken };

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

            _tokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ThrowsAsync(new HttpResponseException(apiResponse));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _query.ManageAccount());
        }

        [Fact]
        public async Task Query_ShouldReturnFalse_IfVAAMArketingPrefernceisDisabled()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");
            var customerResponse = TestDataProvider.GetTestData<CustomerDTO>("Account//CustomerDTO");
            var customerProfileResponse = TestDataProvider.GetTestData<CustomerProfileDTO>("Account//CustomerProfileDocResponse");
            var customerFCMemberResponse = TestDataProvider.GetTestData<FcMemberDetails>("Account\\ProfileInfoResponse");
            var countriesRefResponse = TestDataProvider.GetTestData<IEnumerable<CountryRefData>>("Account//CountriesRefResponse");

            customerProfileResponse.CustomerProfile.Preferences.First(x => x.PreferenceType == "VAAMarketingPermissions").PreferenceValue = "N";

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

            _tokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(authResponse);
            _customerProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);
            _customerTransCustomerProfile.Setup(c => c.CustomerTransactionQueryCustomerProfile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerProfileResponse);
            _customerTransFlyingClub.Setup(c => c.CustomerTransactionQuery(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerFCMemberResponse);
            _countryRefService.Setup(x => x.Query()).ReturnsAsync(countriesRefResponse);

            var result = await _query.ManageAccount();

            //Assert
            Assert.NotNull(result);
            Assert.Equal("CheckFirstNameTesting", result.FirstName);
            Assert.Equal("44", result.CountryCode);
            Assert.Equal("Y", result.Address.First().PreferredFlag);
            Assert.Contains(result.Address, item => item.PostalCode == "E1 1AB");
            Assert.IsType<ManageAccountResponse<AddressCustMS>>(result);
            Assert.False(result.IsVAAMarketingPreferred);
        }

        [Fact]
        public async Task Query_ShouldReturnLinkedMemberType_ForChildHouseHoldMember()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");
            var customerResponse = TestDataProvider.GetTestData<CustomerDTO>("Account//CustomerDTO");
            customerResponse.Customer.AddressDetails.First(x => x.PreferredFlag == "Y").Country = "United States Of America";
            var customerProfileResponse = TestDataProvider.GetTestData<CustomerProfileDTO>("Account//CustomerProfileDocResponse");
            var customerFCMemberResponse = TestDataProvider.GetTestData<FcMemberDetails>("Account\\ProfileInfoResponse");
            var countriesRefResponse = TestDataProvider.GetTestData<IEnumerable<CountryRefData>>("Account//CountriesRefResponse");

            customerFCMemberResponse.FlyingClubMemberDocuments.First().MemberType = "Individual";

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

            _tokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(authResponse);
            _customerProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);
            _customerTransCustomerProfile.Setup(c => c.CustomerTransactionQueryCustomerProfile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerProfileResponse);
            _customerTransFlyingClub.Setup(c => c.CustomerTransactionQuery(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerFCMemberResponse);
            _countryRefService.Setup(x => x.Query()).ReturnsAsync(countriesRefResponse);

            var result = await _query.ManageAccount();

            //Assert
            Assert.NotNull(result);
            Assert.Equal("CheckFirstNameTesting", result.FirstName);
            Assert.Equal("44", result.CountryCode);
            Assert.Equal("Y", result.Address.First().PreferredFlag);
            Assert.Contains(result.Address, item => item.PostalCode == "E1 1AB");
            Assert.IsType<ManageAccountResponse<AddressCustMS>>(result);
            Assert.True(result.IsVAAMarketingPreferred);
            Assert.Equal(HouseHoldMemberType.Linked, result.MemberType);
            Assert.IsType<HouseHoldMemberType>(result.MemberType);
            Assert.Equal("US", result.Address.First().CountryCode);
        }

        [Fact]
        public async Task Query_ShouldReturnIndividualMemberType_ForIndividualCustomer()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");
            var customerResponse = TestDataProvider.GetTestData<CustomerDTO>("Account//CustomerDTO");
            var customerProfileResponse = TestDataProvider.GetTestData<CustomerProfileDTO>("Account//CustomerProfileDocResponse");
            var customerFCMemberResponse = TestDataProvider.GetTestData<FcMemberDetails>("Account\\ProfileInfoResponse");
            var countriesRefResponse = TestDataProvider.GetTestData<IEnumerable<CountryRefData>>("Account//CountriesRefResponse");

            customerFCMemberResponse.FlyingClubMemberDocuments.First().MemberType = "Individual";
            customerFCMemberResponse.FlyingClubMemberDocuments.First().HeadOfGroupMemberNumber = "";


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

            _tokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(authResponse);
            _customerProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);
            _customerTransCustomerProfile.Setup(c => c.CustomerTransactionQueryCustomerProfile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerProfileResponse);
            _customerTransFlyingClub.Setup(c => c.CustomerTransactionQuery(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerFCMemberResponse);
            _countryRefService.Setup(x => x.Query()).ReturnsAsync(countriesRefResponse);

            var result = await _query.ManageAccount();

            //Assert
            Assert.NotNull(result);
            Assert.Equal("CheckFirstNameTesting", result.FirstName);
            Assert.Equal("44", result.CountryCode);
            Assert.Equal("Y", result.Address.First().PreferredFlag);
            Assert.Contains(result.Address, item => item.PostalCode == "E1 1AB");
            Assert.IsType<ManageAccountResponse<AddressCustMS>>(result);
            Assert.True(result.IsVAAMarketingPreferred);
            Assert.Equal(HouseHoldMemberType.Individual, result.MemberType);
            Assert.IsType<HouseHoldMemberType>(result.MemberType);
        }
    }
}
