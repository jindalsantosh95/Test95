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
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class UpdateEmailAddressMutationTests
    {
        private readonly Mock<ITokenB2CProvider<string>> _tokenB2CProvider;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly Mock<ILogger<UpdateEmailAddressMutation>> _logger;
        private readonly Mock<IRefDataService<BlackListedEmails>> _blackListedEmailsRefService;
        private readonly UpdateEmailAddressMutation _mutation;

        public UpdateEmailAddressMutationTests()
        {
            _tokenB2CProvider = new Mock<ITokenB2CProvider<string>>();
            _httpConnector = new Mock<IHttpConnector>();
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _logger = new Mock<ILogger<UpdateEmailAddressMutation>>();
            _blackListedEmailsRefService = new Mock<IRefDataService<BlackListedEmails>>();
            _mutation = new UpdateEmailAddressMutation(_tokenB2CProvider.Object, _logger.Object, _httpConnector.Object, _blackListedEmailsRefService.Object, _tokenProvider.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenB2CProvideObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateEmailAddressMutation(null, _logger.Object, _httpConnector.Object, _blackListedEmailsRefService.Object, _tokenProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggertObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateEmailAddressMutation(_tokenB2CProvider.Object, null, _httpConnector.Object, _blackListedEmailsRefService.Object, _tokenProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenHttpConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateEmailAddressMutation(_tokenB2CProvider.Object, _logger.Object, null, _blackListedEmailsRefService.Object, _tokenProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenBlackListedEmailsRefServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateEmailAddressMutation(_tokenB2CProvider.Object, _logger.Object, _httpConnector.Object, null, _tokenProvider.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateEmailAddressMutation(_tokenB2CProvider.Object, _logger.Object, _httpConnector.Object, _blackListedEmailsRefService.Object, null));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullLoginToken()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<UpdateEmailRequest>("Account//UpdateEmailRequest");

            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _mutation.UpdateEmail(request));
        }

        [Fact]
        public async Task Mutation_ShouldThorwException_AfterFailingtoConnecttoIdentityMS()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<UpdateEmailRequest>("Account//UpdateEmailRequest");
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
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.UpdateEmail(request));
        }

        [Fact]
        public async Task Mutation_ShouldThorwException_ForFailingtoConnecttoReferenceMS()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<UpdateEmailRequest>("Account//UpdateEmailRequest");
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

            var content = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//RefMSConnectionFailResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");
            var blackListedEmailRefResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity, Content = content };

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

            _blackListedEmailsRefService.Setup(x => x.Query()).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(blackListedEmailRefResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<AggregateException>(() => _mutation.UpdateEmail(request));
        }


        [Fact]
        public async Task Mutation_ShouldThorwError_ForBlackListedEmail()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<UpdateEmailRequest>("Account//UpdateEmailRequest");
            request.EmailAddress = "blockmail@testmail.com";

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

            var blackListedEmailRefResponse = TestDataProvider.GetTestData<IEnumerable<BlackListedEmails>>("Account//BlackListedEmails");

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

            _blackListedEmailsRefService.Setup(x => x.Query()).ReturnsAsync(blackListedEmailRefResponse);

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.UpdateEmail(request));
        }

        [Fact]
        public async Task Mutation_Shouldsucceed_ForGoodRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<UpdateEmailRequest>("Account//UpdateEmailRequest");

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

            var blackListedEmailRefResponse = TestDataProvider.GetTestData<IEnumerable<BlackListedEmails>>("Account//BlackListedEmails");

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

            _blackListedEmailsRefService.Setup(x => x.Query()).ReturnsAsync(blackListedEmailRefResponse);

            _httpConnector.Setup(c => c.SendAsync<string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(new Common.Entities.APIResponse<string>
            {
                Response = "Email Update Successful",
                ResponseMessage = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK
                }
            }));

            var result = await _mutation.UpdateEmail(request);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("Email Update Successful", result);
        }
    }
}
