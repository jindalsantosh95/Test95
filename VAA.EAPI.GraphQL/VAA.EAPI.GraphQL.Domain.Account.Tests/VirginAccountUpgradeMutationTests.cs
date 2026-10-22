using HotChocolate;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class VirginAccountUpgradeMutationTests
    {
        private readonly Mock<ITokenB2CProvider<string>> _tokenB2CProvider;
        private readonly Mock<IAccountCustomerService> _customerProvider;
        private readonly Mock<ICustomerMutationProvider<string>> _customerMutationProvider;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Mock<IStampingPNRsHandler> _stampingPNRsHandler;
        private readonly VirginAccountUpgradeMutation _mutation;

        public VirginAccountUpgradeMutationTests()
        {
            _tokenB2CProvider = new Mock<ITokenB2CProvider<string>>();
            _customerProvider = new Mock<IAccountCustomerService>();
            _customerMutationProvider = new Mock<ICustomerMutationProvider<string>>();
            _httpConnector = new Mock<IHttpConnector>();
            _stampingPNRsHandler = new Mock<IStampingPNRsHandler>();
            _mutation = new VirginAccountUpgradeMutation(_tokenB2CProvider.Object, _customerProvider.Object, _customerMutationProvider.Object, _httpConnector.Object, _stampingPNRsHandler.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenB2CProvideObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new VirginAccountUpgradeMutation(null, _customerProvider.Object, _customerMutationProvider.Object, _httpConnector.Object, _stampingPNRsHandler.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new VirginAccountUpgradeMutation(_tokenB2CProvider.Object, null, _customerMutationProvider.Object, _httpConnector.Object, _stampingPNRsHandler.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerMutationProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new VirginAccountUpgradeMutation(_tokenB2CProvider.Object, _customerProvider.Object, null, _httpConnector.Object, _stampingPNRsHandler.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenHttpConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new VirginAccountUpgradeMutation(_tokenB2CProvider.Object, _customerProvider.Object, _customerMutationProvider.Object, null, _stampingPNRsHandler.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenIStampingPNRsHandlerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new VirginAccountUpgradeMutation(_tokenB2CProvider.Object, _customerProvider.Object, _customerMutationProvider.Object, _httpConnector.Object, null));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullLoginToken()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<VirginAccountDetails>("Account//VacUpgradeRequest");

            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _mutation.upgradeVirginAccount(request));
        }

        [Fact]
        public async Task Mutation_ShouldThorwException_AfterFailingtoConnecttoIdentityMSforTokenValidation()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<VirginAccountDetails>("Account//VacUpgradeRequest");

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

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

            _httpConnector.Setup(c => c.SendAsync<EAPIResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new HttpResponseException(apiResponse));

            _tokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.upgradeVirginAccount(request));
        }

        [Fact]
        public async Task Query_ShouldThorwException_ForMissingCustomerDocument()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<VirginAccountDetails>("Account//VacUpgradeRequest");
            var customerResponse = TestDataProvider.GetTestData<CustomerDTO>("Account//CustomerDTO");
            customerResponse = null;
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

            _customerProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);

            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _mutation.upgradeVirginAccount(request));
        }

        [Fact]
        public async Task Query_ShouldThorwException_ForFailingUpgrade()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<VirginAccountDetails>("Account//VacUpgradeRequest");
            var customerResponse = TestDataProvider.GetTestData<CustomerDTO>("Account//CustomerDTO");
            var countryDetailsResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<CountryDetails>>>("Account//CountryDetails");
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

            var content = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//MSConnectionFailResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");
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

            _customerProvider.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);

            _httpConnector.Setup(c => c.GetAsync<IEnumerable<CountryDetails>>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).ReturnsAsync(countryDetailsResponse);

            _customerMutationProvider.Setup(c => c.UpgradeFlyingClub(It.IsAny<EnrolmentRequest>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.upgradeVirginAccount(request));
        }
    }
}
