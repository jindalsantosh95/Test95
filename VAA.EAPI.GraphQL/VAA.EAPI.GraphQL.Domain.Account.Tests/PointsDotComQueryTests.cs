using HotChocolate;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class PointsDotComQueryTests
    {
        private readonly Mock<ITokenB2CProvider<string>> _tokenB2CProvider;
        private readonly Mock<ILogger<PointsDotComQuery>> _logger;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly Mock<IRefDataService<CountryRefData>> _countryRefService;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly PointsDotComQuery _query;

        public PointsDotComQueryTests()
        {
            _countryRefService = new Mock<IRefDataService<CountryRefData>>();
            _httpConnector = new Mock<IHttpConnector>();
            _tokenB2CProvider = new Mock<ITokenB2CProvider<string>>();
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _logger = new Mock<ILogger<PointsDotComQuery>>();
            _query = new PointsDotComQuery(_tokenB2CProvider.Object, _logger.Object, _countryRefService.Object, _httpConnector.Object, _tokenProvider.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_Whentokenb2CProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PointsDotComQuery(null, _logger.Object, _countryRefService.Object, _httpConnector.Object, _tokenProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PointsDotComQuery(_tokenB2CProvider.Object, null, _countryRefService.Object, _httpConnector.Object, _tokenProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCountryRefServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PointsDotComQuery(_tokenB2CProvider.Object, _logger.Object, null, _httpConnector.Object, _tokenProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PointsDotComQuery(_tokenB2CProvider.Object, _logger.Object, _countryRefService.Object, null, _tokenProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PointsDotComQuery(_tokenB2CProvider.Object, _logger.Object, _countryRefService.Object, _httpConnector.Object, null));
        }

        [Fact]
        public async Task Query_ShouldThrowException_ForNullLoginToken()
        {
            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _query.GetPointsDotComDetails("Test"));
        }

        [Fact]
        public async Task Query_ShouldReturnResponse_ForGoodRequest()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

            var customerResponse = TestDataProvider.GetTestData<APIResponse<CustomerDocument>>("Account//CustomerDocumentResponse");
            var customerTransactionResponse = TestDataProvider.GetTestData<APIResponse<CustomerTransactionDocument>>("Account//CustomerTransactionResponse");

            var vredResponse = TestDataProvider.GetTestData<APIResponse<VRedResponse>>("Account//VRedResponse");
            var countriesRefREsponse = TestDataProvider.GetTestData<IEnumerable<CountryRefData>>("Account//CountriesRefResponse");

            var shopAwayTokenResponse = new APIResponse<string>()
            {
                Response = "9999998835|Hundredsix|Pcctest|135922127|autonamejfujuautonamezcxkf@test.com|US|14/05/2019|USD||9999998835|||en|||FF"

            };

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

            _httpConnector.Setup(m => m.GetAsync<CustomerDocument>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).ReturnsAsync(customerResponse);

            _httpConnector.Setup(m => m.GetAsync<CustomerTransactionDocument>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).ReturnsAsync(customerTransactionResponse);

            _httpConnector.Setup(m => m.GetAsync<VRedResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).ReturnsAsync(vredResponse);
            _countryRefService.Setup(x => x.Query()).ReturnsAsync(countriesRefREsponse);

            _httpConnector.Setup(c => c.SendAsync<string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(shopAwayTokenResponse));

            var result = await _query.GetPointsDotComDetails("Buy");

            //Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
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

            _tokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _query.GetPointsDotComDetails("Buy"));
        }

        [Fact]
        public async Task Query_ShouldThorwException_ForFailingtoConnecttoCustomerMS()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

            var content = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//AuthTokenFailedResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");
            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity, Content = content };

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

            _httpConnector.Setup(m => m.GetAsync<CustomerDocument>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _query.GetPointsDotComDetails("Gift"));
        }

        [Fact]
        public async Task Query_ShouldThorwException_ForFailingtoConnecttoReferenceMS()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

            var customerResponse = TestDataProvider.GetTestData<APIResponse<CustomerDocument>>("Account//CustomerDocumentResponse");
            var customerTransactionResponse = TestDataProvider.GetTestData<APIResponse<CustomerTransactionDocument>>("Account//CustomerTransactionResponse");

            var vredResponse = TestDataProvider.GetTestData<APIResponse<VRedResponse>>("Account//VRedResponse");

            var content = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//RefMSConnectionFailResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");
            var countriesRefREsponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity, Content = content };

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

            _httpConnector.Setup(m => m.GetAsync<CustomerDocument>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).ReturnsAsync(customerResponse);

            _httpConnector.Setup(m => m.GetAsync<CustomerTransactionDocument>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).ReturnsAsync(customerTransactionResponse);

            _httpConnector.Setup(m => m.GetAsync<VRedResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).ReturnsAsync(vredResponse);
            _countryRefService.Setup(x => x.Query()).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(countriesRefREsponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _query.GetPointsDotComDetails("Gift"));
        }

        [Fact]
        public async Task Query_ShouldThorwException_ForFailingtoConnecttoSecurityMS()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

            var customerResponse = TestDataProvider.GetTestData<APIResponse<CustomerDocument>>("Account//CustomerDocumentResponse");
            var customerTransactionResponse = TestDataProvider.GetTestData<APIResponse<CustomerTransactionDocument>>("Account//CustomerTransactionResponse");

            var vredResponse = TestDataProvider.GetTestData<APIResponse<VRedResponse>>("Account//VRedResponse");

            var countriesRefREsponse = TestDataProvider.GetTestData<IEnumerable<CountryRefData>>("Account//CountriesRefResponse");

            var content = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//RefMSConnectionFailResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");
            var shopAwayTokenResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity, Content = content };

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

            _httpConnector.Setup(m => m.GetAsync<CustomerDocument>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).ReturnsAsync(customerResponse);

            _httpConnector.Setup(m => m.GetAsync<CustomerTransactionDocument>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).ReturnsAsync(customerTransactionResponse);

            _httpConnector.Setup(m => m.GetAsync<VRedResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).ReturnsAsync(vredResponse);
            _countryRefService.Setup(x => x.Query()).ReturnsAsync(countriesRefREsponse);

            _httpConnector.Setup(c => c.SendAsync<string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(shopAwayTokenResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _query.GetPointsDotComDetails("Transfer"));
        }
    }
}
