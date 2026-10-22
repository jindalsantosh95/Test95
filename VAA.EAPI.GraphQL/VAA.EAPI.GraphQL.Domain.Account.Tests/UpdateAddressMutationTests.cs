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
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class UpdateAddressMutationTests
    {
        private readonly Mock<ITokenB2CProvider<string>> _tokenB2CProvider;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly Mock<ILogger<UpdateAddressMutation>> _logger;
        private readonly UpdateAddressMutation _mutation;
        private readonly Mock<IAccountCustomerService> _customerProvider;
        private readonly Mock<ICustomerTransactionQueryProvider<GroupDocumentDTO>> _customerTransUpdateAddressHH;
        private readonly Mock<IRefDataService<CountryRefData>> _countryRefService;

        public UpdateAddressMutationTests()
        {
            _tokenB2CProvider = new Mock<ITokenB2CProvider<string>>();
            _httpConnector = new Mock<IHttpConnector>();
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _logger = new Mock<ILogger<UpdateAddressMutation>>();
            _customerProvider = new Mock<IAccountCustomerService>();
            _customerTransUpdateAddressHH = new Mock<ICustomerTransactionQueryProvider<GroupDocumentDTO>>();
            _countryRefService = new Mock<IRefDataService<CountryRefData>>();
            _mutation = new UpdateAddressMutation(_tokenB2CProvider.Object, _logger.Object, _httpConnector.Object, _tokenProvider.Object, _customerProvider.Object, _customerTransUpdateAddressHH.Object, _countryRefService.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenB2CProvideObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateAddressMutation(null, _logger.Object, _httpConnector.Object, _tokenProvider.Object, _customerProvider.Object, _customerTransUpdateAddressHH.Object, _countryRefService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggertObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateAddressMutation(_tokenB2CProvider.Object, null, _httpConnector.Object, _tokenProvider.Object, _customerProvider.Object, _customerTransUpdateAddressHH.Object, _countryRefService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenHttpConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateAddressMutation(_tokenB2CProvider.Object, _logger.Object, null, _tokenProvider.Object, _customerProvider.Object, _customerTransUpdateAddressHH.Object, _countryRefService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateAddressMutation(_tokenB2CProvider.Object, _logger.Object, _httpConnector.Object, null, _customerProvider.Object, _customerTransUpdateAddressHH.Object, _countryRefService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateAddressMutation(_tokenB2CProvider.Object, _logger.Object, _httpConnector.Object, _tokenProvider.Object, null, _customerTransUpdateAddressHH.Object, _countryRefService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerTransUpdateAddressHHProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateAddressMutation(_tokenB2CProvider.Object, _logger.Object, _httpConnector.Object, _tokenProvider.Object, _customerProvider.Object, null, _countryRefService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCountryRefServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateAddressMutation(_tokenB2CProvider.Object, _logger.Object, _httpConnector.Object, _tokenProvider.Object, _customerProvider.Object, _customerTransUpdateAddressHH.Object, null));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullLoginToken()
        {
            //Arrange

            var request = TestDataProvider.GetTestData<UpdateMemberAddressRequest>("Account//AccountAddressDetailRequest");

            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _mutation.UpdateMemberAddress(request));
        }

        [Fact]
        public async Task Mutation_ShouldThorwException_AfterFailingtoConnecttoIdentityMS()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<UpdateMemberAddressRequest>("Account//AccountAddressDetailRequest");
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
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.UpdateMemberAddress(request));
        }

        [Fact]
        public async Task Mutation_Shouldsucceed_ForGoodRequestForIndividualMember()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<UpdateMemberAddressRequest>("Account//AccountAddressDetailRequest");
            request.MemberType = HouseHoldMemberType.Individual;
            request.AccountAddressDetail.Country = "GB";

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");
            var customerResponse = TestDataProvider.GetTestData<CustomerDTO>("Account//CustomerDTO");
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

            _countryRefService.Setup(x => x.Query()).ReturnsAsync(countriesRefResponse);

            _httpConnector.Setup(c => c.SendAsync<ManageAccountUpdateResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(new Common.Entities.APIResponse<ManageAccountUpdateResponse>
            {
                Response = new ManageAccountUpdateResponse()
                {
                    Value = "Updated Member Address Successfully"
                }
            }));

            var result = await _mutation.UpdateMemberAddress(request);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Member Address Successfully", result);
        }

        [Fact]
        public async Task Mutation_Shouldsucceed_ForGoodRequestForHouseHoldandLinkedMembers()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<UpdateMemberAddressRequest>("Account//AccountAddressDetailRequest");
            request.MemberType = HouseHoldMemberType.Household;
            request.AccountAddressDetail.Country = "US";

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");
            var customerResponse = TestDataProvider.GetTestData<CustomerDTO>("Account//CustomerDTO");

            var customerTransactionResponse = TestDataProvider.GetTestData<GroupDocumentDTO>("Account//GroupDocumentResponse");
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

            _customerTransUpdateAddressHH.Setup(c => c.CustomerTransactionUpdateAddressHouseHold(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerTransactionResponse);

            _countryRefService.Setup(x => x.Query()).ReturnsAsync(countriesRefResponse);

            _httpConnector.Setup(c => c.SendAsync<ManageAccountUpdateResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(new Common.Entities.APIResponse<ManageAccountUpdateResponse>
            {
                Response = new ManageAccountUpdateResponse()
                {
                    Value = "Updated Member Address Successfully"
                }
            }));

            var result = await _mutation.UpdateMemberAddress(request);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Member Address Successfully", result);
        }

        [Fact]
        public async Task Mutation_ShouldThrow_AfterFailingToCoonecttoCustomerMS()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<UpdateMemberAddressRequest>("Account//AccountAddressDetailRequest");

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

            var content = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//RefMSConnectionFailResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");

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

            _httpConnector.Setup(c => c.SendAsync<string>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.UpdateMemberAddress(request));
        }

        [Fact]
        public async Task Mutation_ShouldThrowError_ForMissingPostalCodeForUK()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<UpdateMemberAddressRequest>("Account//AccountAddressDetailRequest");
            request.AccountAddressDetail.PostalCode = string.Empty;
            request.AccountAddressDetail.Country = "GB";

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.UpdateMemberAddress(request));
        }

        [Fact]
        public async Task Mutation_ShouldThrowError_ForMissingPostalCodeForUS()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<UpdateMemberAddressRequest>("Account//AccountAddressDetailRequest");
            request.AccountAddressDetail.PostalCode = null;
            request.AccountAddressDetail.Country = "US";

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.UpdateMemberAddress(request));
        }
    }
}
