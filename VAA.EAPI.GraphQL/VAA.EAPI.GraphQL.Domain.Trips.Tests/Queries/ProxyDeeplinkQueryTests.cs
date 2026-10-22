using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Queries.DeepLink;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Queries
{
    public class ProxyDeeplinkQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly ILogger<ProxyDeeplinkQuery> _loggerDeepLink;
        private readonly HttpConnector _httpConnector;
        private readonly ITripsDetailsQueryProvider _mockTripsDetailsQueryProvider;
        private readonly ProxyDeeplinkQuery _deepLinkQuery;


        public ProxyDeeplinkQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerDeepLink = Microsoft.Extensions.Logging.Abstractions.NullLogger<ProxyDeeplinkQuery>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _mockTripsDetailsQueryProvider = new TripsDetailsQueryProvider(_httpConnector);
            _deepLinkQuery = new ProxyDeeplinkQuery(_mockTripsDetailsQueryProvider, _loggerDeepLink);
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task ProxyDeeplinkQuery_ValidchacheId_ReturnsResponse()
        {
            // Arrange
            string cacheId = "737afc2d-91d5-41b4-9b93-c52b4673dee8";

            // Act
            var result = await _deepLinkQuery.GetDeepLinkDetails(cacheId);

            // Assert
            Assert.NotNull(result);
            Assert.True(!string.IsNullOrEmpty(result.Endpoint.EndpointUrl));
            Assert.Equal("https://stg.virginatlantic.com/my-trips/search",result.Endpoint.EndpointUrl);
            Assert.True(!string.IsNullOrEmpty(result.Endpoint.Http_Method));
            Assert.Equal("POST", result.Endpoint.Http_Method);
            Assert.True(result.Parameters != null && result.Parameters.Any());
            Assert.True(result.Parameters.Any(p => p.ParamName == "PNR" && p.ParamValue == "SDSDSD"));
            Assert.True(result.Parameters.Any(p => p.ParamName == "FirstName" && p.ParamValue == "testFristName"));
            Assert.True(result.Parameters.Any(p => p.ParamName == "LastName" && p.ParamValue == "testLastName"));
        }
    }
    
}
