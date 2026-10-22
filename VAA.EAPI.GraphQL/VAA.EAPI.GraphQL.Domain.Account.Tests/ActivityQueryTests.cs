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
using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class ActivityQueryTests
    {
        private readonly ActivityQuery _query;
        private readonly Mock<ITokenB2CProvider<string>> _tokenB2CProvider;
        private readonly Mock<ILogger<ActivityQuery>> _logger;
        private readonly Mock<IActivityQueryProvider> _activityQueryProvider;

        public ActivityQueryTests()
        {
            _tokenB2CProvider = new Mock<ITokenB2CProvider<string>>();
            _logger = new Mock<ILogger<ActivityQuery>>();
            _query = new ActivityQuery(_tokenB2CProvider.Object, _logger.Object);
            _activityQueryProvider = new Mock<IActivityQueryProvider>();
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenB2CProvideObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActivityQuery(null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActivityQuery(_tokenB2CProvider.Object, null));
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenNoObjectspassed()
        {
            Assert.Throws<ArgumentNullException>(() => new ActivityQuery(null, null));
        }

        [Fact]
        public async Task Query_ShouldThrowException_ForNullLoginToken()
        {
            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _query.ActivityDetails(_activityQueryProvider.Object, 1, 10));

        }

        [Fact]
        public async Task Query_ShouldReturnError_AfterFailingtoConnecttoIdentityMS()
        {
            //Arrange
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

            _tokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _query.ActivityDetails(_activityQueryProvider.Object, 1, 10));
        }

        [Fact]
        public async Task Query_ShouldReturnActivityStatemnet_ForGoodRequest()
        {
            //Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");

            //Act

            // Configure TestContext first then set to current AsyncContext
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", loginToken },
                {"Page", 1 },
                {"PageSize", 10 }
            };

            var testContext = new TestFunctionContext(bindingData);

            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));

            AsyncContext.Current = new AsyncContext() { Context = testContext };

            IList<ActivityDetail> activityDetails =
            [
                new ActivityDetail()
                {

                    ActivityDate = DateTime.UtcNow.AddDays(-30),
                    PartnerName = "Virgin Atlantic Airways",
                    Statement = "Virgin Atlantic Airways, LHR - SAN, Economy Classic",
                    VirginPoints = "28,000",
                    TierPoints = "0",
                    TransactionProcessedDate = DateTime.UtcNow.AddMonths(-3),
                    TransactionId = "2136343646458",
                    PartnerCode = "VS",
                    TransactionType = "Redemption"
                },
            ];

            _tokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(authResponse);
            _activityQueryProvider.Setup(x => x.GetActivitiesQueryData(It.IsAny<TokenB2CResponse>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(activityDetails);

            var result = await _query.ActivityDetails(_activityQueryProvider.Object, 1, 10);

            //Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, item => item.TransactionId == "2136343646458");
            Assert.Contains(result, item => item.PartnerCode == "VS");
            Assert.Contains(result, item => item.PartnerName == "Virgin Atlantic Airways");
        }
    }
}
