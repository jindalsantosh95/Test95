using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Interfaces;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Models;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Queries;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Tests
{
    public class NDCHealthCheckQueryTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IHeartBeatService _heartBeatService;
        private readonly IHealthReportService _healthReportService;
        private readonly HttpConnector _httpConnector;
        private readonly NDCHealthCheckQuery _query;
        private readonly ILogger<HttpConnector> _logger;

        public NDCHealthCheckQueryTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _heartBeatService = new HeartBeatService(_httpConnector);
            _healthReportService = new HealthReportService(_httpConnector);
            _query = new NDCHealthCheckQuery(_heartBeatService, _healthReportService);

            ConfigReader.Set("NDCOfferHeartBeatUrl", "https://Testresources.virginatlantic.com/DEV/SalesAndRevenueManagement/OrderShopping/OfferManagement/v2/heartbeattest");
            ConfigReader.Set("NDCOrderHeartBeatUrl", "https://Testresources.virginatlantic.com/DEV/SalesAndRevenueManagement/OrderShopping/OrderManagement/v2/heartbeattest");
            ConfigReader.Set("NDCHealth:NDCHealthReportUrl", "https://Testresources.virginatlantic.com/DEV/NDCHealthMS/HealthReportTest");
            ConfigReader.Set("NDCHealth:NDCHealthReportSubKey", "345");
            ConfigReader.Set("NDCHealth:PaymentFacadeHeartBeatUrl", "https://Testresources.virginatlantic.com/DEV/IFGFacade/heartbeattest");
            ConfigReader.Set("NDCHealth:PaymentFacadeSubKey", "123");
            ConfigReader.Set("NDCHealth:PaymentFacadeTokenClientId", "client_id-abc");
            ConfigReader.Set("NDCHealth:PaymentFacadeTokenClientSecret", "client_secret-123");
            ConfigReader.Set("NDCHealth:PaymentFacadeTokenResource", "resource-123");
            ConfigReader.Set("NDCHealth:PaymentFacadeTokenUrl", "https://paymentfacade.com/token");

            ConfigReader.Set("NDCHealth:LowThreshold", "5");
            ConfigReader.Set("NDCHealth:Threshold", "10");
            ConfigReader.Set("NDCHealth:HighThreshold", "20");
        }

        [Fact]
        public void Constructor_ThrowException_NullReferences()
        {
            Assert.Throws<ArgumentNullException>(() => new NDCHealthCheckQuery(null, _healthReportService));
        }

        [Fact]
        public async Task HealthStatusTest()
        {
            //Arrange
            var input = new List<string> { "AirShopping", "OfferPrice", "OrderQuote", "OrderReshop", "SeatAvailability", "ServiceList", "OrderCreate", "OrderChange", "OrderRetrieve", "OrderChangeNotif" };

            //Act
            var result = await _query.GetHealthReport(input);

            //Assert
            Assert.Equal(10 , result.Count());
            Assert.Equal(RAGStatus.Amber, result.First(x => x.Api == "AirShopping").Status);
            Assert.Equal(RAGStatus.Red, result.First(x => x.Api == "AirShopping").Remarks.First().Status);
            Assert.Equal(RAGStatus.Green, result.First(x => x.Api == "OfferPrice").Status);
            Assert.Equal(RAGStatus.Green, result.First(x => x.Api == "OrderQuote").Status);
            Assert.Equal(RAGStatus.Green, result.First(x => x.Api == "OrderReshop").Status);
            Assert.Equal(RAGStatus.Green, result.First(x => x.Api == "SeatAvailability").Status);
            Assert.Equal(RAGStatus.Red, result.First(x => x.Api == "ServiceList").Status);
            Assert.Equal(RAGStatus.Amber, result.First(x => x.Api == "OrderCreate").Status);
            Assert.NotEmpty(result.First(x => x.Api == "OrderCreate").Remarks.First().ImpactedFunctionality.ToString());
            Assert.NotEmpty(result.First(x => x.Api == "OrderChange").Remarks.First().ImpactedFunctionality.ToString());
            Assert.Equal(RAGStatus.Green, result.First(x => x.Api == "OrderRetrieve").Status);
            Assert.Equal(RAGStatus.Green, result.First(x => x.Api == "OrderChangeNotif").Status);

        }
    }
}