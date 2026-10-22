using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Services
{
    public class OrderDataServiceProviderTests: IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpConnector _httpConnector;
        private readonly OrderDataServiceProvider _provider;
        private readonly ILogger<HttpConnector> _logger;

        public OrderDataServiceProviderTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _provider = new OrderDataServiceProvider(_httpConnector);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenConnectorIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new OrderDataServiceProvider(null));
        }

        [Fact]
        public async Task GetTripDetails_ShouldThrow_WhenRequestIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _provider.GetTripDetails(null));
        }

        [Fact]
        public async Task GetTripDetails_ShouldReturnExpectedBookings()
        {
            var request = new Infrastructure.Models.OrderDataMS.TripDetailsRequest
            {
                TripDetails =
                [
                   new Infrastructure.Models.OrderDataMS.TripDetails { FirstName = "ANGELIC", LastName = "KOPACZ", PnrLocator = "FD2D95", CreatedDate = "15OCT25" }
                ]
            };

            var expectedResponse = new List<Infrastructure.Models.TripMS.Booking>
            {
                new() {
                   Pos = new Infrastructure.Models.TripMS.Pos { CityCode = "LON" },
                   Product =
                   [
                      new Infrastructure.Models.TripMS.Product
                      {
                         Reference = new Infrastructure.Models.TripMS.Reference
                         {
                            AirlineDesignator = "VS",
                            PnrLocator = "FD2D95",
                            CreatedDate = DateTime.ParseExact("15OCT25", "ddMMMyy", CultureInfo.InvariantCulture)
                         }
                      }
                   ],
                   Passengers =
                   [
                      new Infrastructure.Models.TripMS.Passenger
                      {
                         CustomerId = "9999215370074",
                         FirstName = "ANGELIC",
                         LastName = "KOPACZ",
                         PassengerType = new Infrastructure.Models.TripMS.PassengerType { Code = "A" }
                      }
                   ]
                }
            };

            var httpResponse = new APIResponse<IEnumerable<Infrastructure.Models.TripMS.Booking>>
            {
                Response = expectedResponse
            };

            // Act
            var result = await _provider.GetTripDetails(request);

            // Assert
            Assert.NotNull(result);
            var expectedList = expectedResponse.ToList();
            var actualList = result.ToList();
            Assert.Equal(expectedList.Count, actualList.Count);
            Assert.Equal(expectedList[0].Passengers.First().FirstName, actualList[0].Passengers.First().FirstName);
            Assert.Equal(expectedList[0].Product.First().Reference.PnrLocator, actualList[0].Product.First().Reference.PnrLocator);
        }
    }
}