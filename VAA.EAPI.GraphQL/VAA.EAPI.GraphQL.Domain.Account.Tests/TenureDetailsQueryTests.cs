using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Domain.Account.Tests.Helpers;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class TenureDetailsQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IMemberCustomAttributeProvider _memberCustomAttributeProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IAccountCustomerService _customerProvider;
        private readonly TimeProvider _timeProvider;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<TenureDetailsQuery> _loggerQuery;
        private readonly TenureDetailsQuery _query;

        public TenureDetailsQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            var customerService = new CustomerService(_tokenProvider.Object, _httpConnector);
            _tokenB2CProvider = new TokenB2CProvider<string>(_tokenProvider.Object, _httpConnector, customerService, Microsoft.Extensions.Logging.Abstractions.NullLogger<TokenB2CProvider<string>>.Instance);
            _loggerQuery = Microsoft.Extensions.Logging.Abstractions.NullLogger<TenureDetailsQuery>.Instance;
            _memberCustomAttributeProvider = new MemberCustomAttributeProvider(_tokenProvider.Object, _httpConnector);
            _customerProvider = customerService;
            _timeProvider = new FixedTimeProvider(new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero));
            _query = new TenureDetailsQuery(_loggerQuery, _memberCustomAttributeProvider, _tokenB2CProvider, _customerProvider, _timeProvider);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenB2CProvideObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TenureDetailsQuery(_loggerQuery, _memberCustomAttributeProvider, null, _customerProvider, _timeProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomAttributeProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TenureDetailsQuery(_loggerQuery, null, _tokenB2CProvider, _customerProvider, _timeProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TenureDetailsQuery(null, _memberCustomAttributeProvider, _tokenB2CProvider, _customerProvider, _timeProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TenureDetailsQuery(_loggerQuery, _memberCustomAttributeProvider, _tokenB2CProvider, null, _timeProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTimeProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TenureDetailsQuery(_loggerQuery, _memberCustomAttributeProvider, _tokenB2CProvider, _customerProvider, null));
        }

        [Fact]
        public async Task Query_ShouldThrowException_ForEmptyToken()
        {
            //Arrange
            var loginToken = string.Empty;

            //Act
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
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/ValidateAuthToken/good");

            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _query.AccountTenureDetails());
        }

        [Fact]
        public async Task Query_ShouldThrowException_ForInvalidToken()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

            //Act
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
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/ValidateAuthToken/Bad");

            //Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _query.AccountTenureDetails());
        }

        [Fact]
        public async Task Query_Success_NoFlightsInResponse()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

            //Act
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

            ConfigReader.Set(Common.Constants.Environment.CustTransactionMsUrl, "https://testresources.virginatlantic.com/dev/customertransactions/inactive/");

            //Act
            var result = await _query.AccountTenureDetails();

            //Assert
            Assert.NotNull(result.MemberCustomAttributesDocuments);
            Assert.IsType<MemberCustomAttributesResponseAccount>(result);
            Assert.False(result.MemberCustomAttributesDocuments.First().IsAnActiveYear);
            Assert.Empty(result.MemberCustomAttributesDocuments.First().MemberLastFiveVSActivityYear);
        }

        [Fact]
        public async Task Query_Success_WithActiveYear()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

            //Act
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

            ConfigReader.Set(Common.Constants.Environment.CustTransactionMsUrl, "https://testresources.virginatlantic.com/dev/customertransactions/active/");

            //Act
            var result = await _query.AccountTenureDetails();

            //Assert
            Assert.NotNull(result.MemberCustomAttributesDocuments);
            Assert.IsType<MemberCustomAttributesResponseAccount>(result);
            Assert.True(result.MemberCustomAttributesDocuments.First().IsAnActiveYear);
            Assert.Equal(5, result.MemberCustomAttributesDocuments.First().MemberLastFiveVSActivityYear.Count());

            var years = result.MemberCustomAttributesDocuments
                            .First()
                            .MemberLastFiveVSActivityYear
                            .Select(x => int.Parse(x.ActivityYear))
                            .ToList();

            Assert.True(years.SequenceEqual(years.OrderBy(x => x)),
                "Activity years are not in order.");
        }
    }
}
