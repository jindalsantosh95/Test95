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
using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;
using VAA.EAPI.GraphQL.Domain.Account.Mutations.HouseHoldAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class InviteActionMutationTests
    {
        private readonly Mock<ITokenB2CProvider<string>> _tokenB2CProvider;
        private readonly Mock<ILogger<InviteActionMutation>> _logger;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly Mock<IAccountCustomerService> _customerProvider;
        private readonly Mock<IHouseHoldMutationProvider> _houseHoldMutationProvider;
        private readonly Mock<ICustomerTransactionQueryProvider<string>> _customerTransDeleteInviteDoc;
        private readonly InviteActionMutation _mutation;

        public InviteActionMutationTests()
        {
            _tokenB2CProvider = new Mock<ITokenB2CProvider<string>>();
            _logger = new Mock<ILogger<InviteActionMutation>>();
            _httpConnector = new Mock<IHttpConnector>();
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _customerProvider = new Mock<IAccountCustomerService>();
            _houseHoldMutationProvider = new Mock<IHouseHoldMutationProvider>();
            _customerTransDeleteInviteDoc = new Mock<ICustomerTransactionQueryProvider<string>>();
            _mutation = new InviteActionMutation(_tokenB2CProvider.Object, _logger.Object, _tokenProvider.Object, _httpConnector.Object, _customerProvider.Object, _customerTransDeleteInviteDoc.Object, _houseHoldMutationProvider.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenB2CProvideObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new InviteActionMutation(null, _logger.Object, _tokenProvider.Object, _httpConnector.Object, _customerProvider.Object, _customerTransDeleteInviteDoc.Object, _houseHoldMutationProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggertObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new InviteActionMutation(_tokenB2CProvider.Object, null, _tokenProvider.Object, _httpConnector.Object, _customerProvider.Object, _customerTransDeleteInviteDoc.Object, _houseHoldMutationProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new InviteActionMutation(_tokenB2CProvider.Object, _logger.Object, null, _httpConnector.Object, _customerProvider.Object, _customerTransDeleteInviteDoc.Object, _houseHoldMutationProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenHttpConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new InviteActionMutation(_tokenB2CProvider.Object, _logger.Object, _tokenProvider.Object, null, _customerProvider.Object, _customerTransDeleteInviteDoc.Object, _houseHoldMutationProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new InviteActionMutation(_tokenB2CProvider.Object, _logger.Object, _tokenProvider.Object, _httpConnector.Object, null, _customerTransDeleteInviteDoc.Object, _houseHoldMutationProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerTransactionQueryProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new InviteActionMutation(_tokenB2CProvider.Object, _logger.Object, _tokenProvider.Object, _httpConnector.Object, _customerProvider.Object, null, _houseHoldMutationProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenHouseHoldMutationProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new InviteActionMutation(_tokenB2CProvider.Object, _logger.Object, _tokenProvider.Object, _httpConnector.Object, _customerProvider.Object, _customerTransDeleteInviteDoc.Object, null));
        }


        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullLoginToken()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<HouseHoldInviteActionRequest>("Account//HouseHoldInviteActionRequest");

            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _mutation.HouseHoldInviteAction(request));
        }

        [Fact]
        public async Task Mutation_ShouldThorwException_AfterFailingtoConnecttoIdentityMSforTokenValidation()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<HouseHoldInviteActionRequest>("Account//HouseHoldInviteActionRequest");

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authToken = TestDataProvider.GetTestData<string>("Account//AuthToken");

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

            _tokenProvider.Setup(_ => _.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync(authToken);

            _httpConnector.Setup(c => c.SendAsync<EAPIResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new HttpResponseException(apiResponse));

            _tokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.HouseHoldInviteAction(request));
        }

        [Fact]
        public async Task Mutation_ShouldSucceed_ForGoodRequestWhenAccepted()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<HouseHoldInviteActionRequest>("Account//HouseHoldInviteActionRequest");

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

            var customerResponse = TestDataProvider.GetTestData<CustomerDTO>("Account//CustomerDTO");

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

            _httpConnector.Setup(c => c.SendAsync<string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Common.Entities.APIResponse<string> { Response = "" });

            _customerTransDeleteInviteDoc.Setup(c => c.CustomerTransactionCreateHouseHoldInviteDoc(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CustomerDTO>(), It.IsAny<CustomerDTO>())).ReturnsAsync("Successfully deleted household invite document.");

            _houseHoldMutationProvider.Setup(c => c.HouseHoldLinking(It.IsAny<HouseHoldLinkingRequest>())).ReturnsAsync(new ResponseMessage() { Message = "Request successfully initiated" });

            _customerProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);

            var result = await _mutation.HouseHoldInviteAction(request);

            //Assert
            Assert.IsType<string>(result);
            Assert.Equal("Successfully Accepted HouseHold Invite", result);
        }


        [Fact]
        public async Task Mutation_ShouldSucceed_ForGoodRequestWhenCancelled()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<HouseHoldInviteActionRequest>("Account//HouseHoldInviteActionRequest");
            request.ActionType = InviteActionType.Cancelled;

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

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

            _httpConnector.Setup(c => c.SendAsync<string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Common.Entities.APIResponse<string> { Response = "" });

            _customerTransDeleteInviteDoc.Setup(c => c.CustomerTransactionCreateHouseHoldInviteDoc(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CustomerDTO>(), It.IsAny<CustomerDTO>())).ReturnsAsync("Successfully deleted household invite document.");

            var result = await _mutation.HouseHoldInviteAction(request);

            //Assert
            Assert.IsType<string>(result);
            Assert.Equal("Successfully Cancelled HouseHold Invite", result);
        }


        [Fact]
        public async Task Mutation_ShouldSucceed_ForGoodRequestWhenCDeclined()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<HouseHoldInviteActionRequest>("Account//HouseHoldInviteActionRequest");
            request.ActionType = InviteActionType.Declined;

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

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

            _httpConnector.Setup(c => c.SendAsync<string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Common.Entities.APIResponse<string> { Response = "" });

            _customerTransDeleteInviteDoc.Setup(c => c.CustomerTransactionCreateHouseHoldInviteDoc(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CustomerDTO>(), It.IsAny<CustomerDTO>())).ReturnsAsync("Successfully deleted household invite document.");

            var result = await _mutation.HouseHoldInviteAction(request);

            //Assert
            Assert.IsType<string>(result);
            Assert.Equal("Successfully Declined HouseHold Invite", result);
        }

        [Fact]
        public async Task Mutation_ShouldThorwException_ForFailingtoConnecttoCustomerMS()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<HouseHoldInviteActionRequest>("Account//HouseHoldInviteActionRequest");

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

            _httpConnector.Setup(c => c.SendAsync<string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new Common.Entities.APIResponse<string> { Response = "" });

            _customerTransDeleteInviteDoc.Setup(c => c.CustomerTransactionCreateHouseHoldInviteDoc(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CustomerDTO>(), It.IsAny<CustomerDTO>())).ReturnsAsync("Successfully deleted household invite document.");

            _houseHoldMutationProvider.Setup(c => c.HouseHoldLinking(It.IsAny<HouseHoldLinkingRequest>())).ReturnsAsync(new ResponseMessage() { Message = "Request successfully initiated" });

            _customerProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new HttpResponseException(apiResponse));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.HouseHoldInviteAction(request));
        }
    }
}
