using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class ActivityQueryProviderTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<ActivityQueryProvider> _logger;
        private readonly IHttpConnector _httpConnector;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ICustomerTransactionQueryProvider<TransactionResponse<TransactionDoc>> _customerTransTransaction;
        private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _customerTransFlyingClub;
        private readonly IRefDataService<ProductDetails> _productRefService;
        private readonly IRefDataService<PartnerProfileDetails> _partnerProfileService;
        private readonly ActivityQueryProvider _queryProvider;

        public ActivityQueryProviderTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ActivityQueryProvider>.Instance;
            _httpConnector = new HttpConnector(Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance, httpClientFactoryFixture.Current);
            _tokenProvider = new TestOAuthTokenProvider();
            var customerService = new CustomerService(_tokenProvider, _httpConnector);
            _tokenB2CProvider = new TokenB2CProvider<string>(_tokenProvider,_httpConnector,customerService,Microsoft.Extensions.Logging.Abstractions.NullLogger<TokenB2CProvider<string>>.Instance);
            _customerProvider = customerService;
            _customerTransTransaction = new CustomerTransactionQueryProvider<TransactionResponse<TransactionDoc>>(_tokenProvider, _httpConnector);
            _customerTransFlyingClub = new CustomerTransactionQueryProvider<FcMemberDetails>(_tokenProvider, _httpConnector);
            _productRefService = new RefDataService<ProductDetails>(_httpConnector);
            _partnerProfileService = new RefDataService<PartnerProfileDetails>(_httpConnector);

            _queryProvider = new ActivityQueryProvider(
                _logger,
                _tokenB2CProvider,
                _httpConnector,
                _customerProvider,
                _customerTransTransaction,
                _productRefService,
                _partnerProfileService,
                _tokenProvider,
                _customerTransFlyingClub,
                new FixedTimeProvider(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTokenB2CProvideObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActivityQueryProvider(_logger, null, _httpConnector, _customerProvider, _customerTransTransaction, _productRefService, _partnerProfileService, _tokenProvider, _customerTransFlyingClub, TimeProvider.System));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenHttpConnectorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActivityQueryProvider(_logger, _tokenB2CProvider, null, _customerProvider, _customerTransTransaction, _productRefService, _partnerProfileService, _tokenProvider, _customerTransFlyingClub, TimeProvider.System));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ActivityQueryProvider(_logger, _tokenB2CProvider, _httpConnector, null, _customerTransTransaction, _productRefService, _partnerProfileService, _tokenProvider, _customerTransFlyingClub, TimeProvider.System));
        }

        [Fact]
        public async Task Query_ShouldReturnActivityStatemnetwithRightPageSize_ForGoodRequest()
        {
            VAA.EAPI.GraphQL.Common.Helper.ConfigReader.Set(VAA.EAPI.GraphQL.Common.Constants.Environment.CustTransactionMsUrl, "https://testresources.virginatlantic.com/dev/customertransactions/activity/");
            VAA.EAPI.GraphQL.Common.Helper.ConfigReader.Set(VAA.EAPI.GraphQL.Common.Constants.Environment.VredMsUrl, "https://testresources.virginatlantic.com/dev/vred/activity");

            var result = await _queryProvider.GetActivitiesQueryData(new TokenB2CResponse
            {
                FcNumber = "1074840875"
            }, 1, 2);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, item => item.TransactionId == "170135829219294653");
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("dummy-token");
        }

        private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => utcNow;
        }
    }
}
