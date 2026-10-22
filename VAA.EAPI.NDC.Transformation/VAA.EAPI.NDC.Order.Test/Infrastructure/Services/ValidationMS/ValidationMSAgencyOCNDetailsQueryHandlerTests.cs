using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using Xunit;

namespace VAA.EAPI.NDC.Order.Tests.Infrastructure.Services.ValidationMS
{
    public class ValidationMSAgencyOCNDetailsQueryHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private const string AggregatorBaseUrl = "https://Testresources.virginatlantic.com/dev/validationmicroservice/AggregatorIdTest/AgencyOCNDetails/";
        private const string NoAggregatorBaseUrl = "https://Testresources.virginatlantic.com/dev/validationmicroservice/NoAggregatorIdTest/AgencyOCNDetails/";
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly ValidationMSAgencyOCNDetailsQueryHandler _handler;

        public ValidationMSAgencyOCNDetailsQueryHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _httpClientFactoryFixture = httpClientFactoryFixture;
            ConfigReader.Set(Constants.Environment.ValidationMSSubscriptionKey, "test-key");
            ConfigReader.Set(Constants.Environment.ValidationMSAgencyOCNDetailsUrl, "https://Testresources.virginatlantic.com/sit/validationmicroservice/AgencyOCNDetails/");
            _handler = new ValidationMSAgencyOCNDetailsQueryHandler(
                new HttpConnector(Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance, httpClientFactoryFixture.Current));
        }

        [Fact]
        public async Task Handle_WithAggregatorId_AppendsAggregatorIdQueryParameter()
        {
            var originalBaseUrl = ConfigReader.Get(Constants.Environment.ValidationMSAgencyOCNDetailsUrl);
            ConfigReader.Set(Constants.Environment.ValidationMSAgencyOCNDetailsUrl, AggregatorBaseUrl);

            try
            {
                const string requestUrl = AggregatorBaseUrl + "00003333?aggregatorId=AGGREGATOR1";
                var responsesBefore = _httpClientFactoryFixture.Current.GetResponses(requestUrl)
                    .Count(response => string.Equals(response.RequestMessage?.RequestUri?.AbsoluteUri, requestUrl, StringComparison.OrdinalIgnoreCase));

                await _handler.Handle(new ValidationMSAgencyOCNDetailsQuery
                {
                    AgencyId = "00003333",
                    AggregatorId = "AGGREGATOR1",
                    Environment = "dev"
                }, CancellationToken.None);

                Assert.Equal(responsesBefore + 1, _httpClientFactoryFixture.Current.GetResponses(requestUrl)
                    .Count(response => string.Equals(response.RequestMessage?.RequestUri?.AbsoluteUri, requestUrl, StringComparison.OrdinalIgnoreCase)));
            }
            finally
            {
                ConfigReader.Set(Constants.Environment.ValidationMSAgencyOCNDetailsUrl, originalBaseUrl);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task Handle_WithoutAggregatorId_UsesLegacyUri(string? aggregatorId)
        {
            var originalBaseUrl = ConfigReader.Get(Constants.Environment.ValidationMSAgencyOCNDetailsUrl);
            ConfigReader.Set(Constants.Environment.ValidationMSAgencyOCNDetailsUrl, NoAggregatorBaseUrl);

            try
            {
                const string requestUrl = NoAggregatorBaseUrl + "00003333";
                var responsesBefore = _httpClientFactoryFixture.Current.GetResponses(requestUrl)
                    .Count(response => string.Equals(response.RequestMessage?.RequestUri?.AbsoluteUri, requestUrl, StringComparison.OrdinalIgnoreCase));

                await _handler.Handle(new ValidationMSAgencyOCNDetailsQuery
                {
                    AgencyId = "00003333",
                    AggregatorId = aggregatorId,
                    Environment = "dev"
                }, CancellationToken.None);

                Assert.Equal(responsesBefore + 1, _httpClientFactoryFixture.Current.GetResponses(requestUrl)
                    .Count(response => string.Equals(response.RequestMessage?.RequestUri?.AbsoluteUri, requestUrl, StringComparison.OrdinalIgnoreCase)));
            }
            finally
            {
                ConfigReader.Set(Constants.Environment.ValidationMSAgencyOCNDetailsUrl, originalBaseUrl);
            }
        }
    }
}
