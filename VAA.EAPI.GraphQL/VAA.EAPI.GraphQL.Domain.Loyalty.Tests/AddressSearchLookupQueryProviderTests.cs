using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.AddressSearch;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class AddressSearchLookupQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHttpConnector _httpConnector;
        private readonly AddressSearchLookupQueryProvider _query;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;

        public AddressSearchLookupQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _query = new AddressSearchLookupQueryProvider(_tokenProvider.Object, _httpConnector);

            Setup.CreateTestEnvironmentSettings();

            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");

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
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new AddressSearchLookupQueryProvider(null, _httpConnector));
            Assert.Throws<ArgumentNullException>(() => new AddressSearchLookupQueryProvider(_tokenProvider.Object, null));
        }

        [Fact]
        public async Task GetAddressSearchbyPostcodeShouldSucceed()
        {
            var queryResponse = await _query.GetAddressSearchbyPostcode(new AddressSearchbyPostcodeRequest
            {
                CountryCode = "GB",
                PostCode = "RH10 9BH"
            });

            Assert.IsType<List<AddressSearchbyPostcodeResponse>>(queryResponse);
            Assert.NotNull(queryResponse);
        }
        [Fact]
        public async Task GetAddressSearchbyRetrieveShouldSucceed()
        {
            var queryResponse = await _query.GetAddressSearchbyRetrieve("GB|RM|B|54367428|ENG");

            Assert.IsType<List<AddressSearchbyRetrieveResponse>>(queryResponse);
            Assert.NotNull(queryResponse);
        }

        [Fact]
        public async Task ThrowException_For_NullCountryCodeGetAddressSearchbyPostcode()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _query.GetAddressSearchbyPostcode(new AddressSearchbyPostcodeRequest
            {
                CountryCode = "",
                PostCode = "RH10 9BH"
            }));
        }

        [Fact]
        public async Task ThrowException_For_NullPostcodeCode_GetAddressSearchbyPostcode()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _query.GetAddressSearchbyPostcode(new AddressSearchbyPostcodeRequest
            {
                CountryCode = "GB",
                PostCode = ""
            }));
        }

        [Fact]
        public async Task ThrowException_For_NullPostcodeCode_GetAddressSearchbyRetrieve()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _query.GetAddressSearchbyRetrieve(""));
        }

    }
}
