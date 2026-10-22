using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Deeplink;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;
using VAA.EAPI.GraphQL.Domain.Trips.Queries.DeepLink;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Queries
{
    public class DeeplinkUrlQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public DeeplinkUrlQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<ITripsDetailsQueryProvider, TripsDetailsQueryProvider>()
                                                      .AddSingleton<IBasketProvider, BasketProvider>()
                                                      .AddSingleton<DeeplinkUrlQuery>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenDependenciesAreNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DeeplinkUrlQuery(null, null));
        }

        [Fact]
        public async Task GetDeeplinkUrl_TravelInsurance_ReturnsExpectedResponse()
        {
            // Arrange
            var deeplinkUrlQuery = _serviceProvider.GetService<DeeplinkUrlQuery>()!;
            var tripReservationId = await BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, "4d7e9512-f5d2-4572-84e1-b2fcdd773993");
            var request = new DeeplinkRequest
            {
                TripReservationId = tripReservationId,
                DeeplinkOption = DeeplinkOption.TravelInsurance
            };

            // Act
            var result = await deeplinkUrlQuery.GetDeeplinkUrl(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://www.magroup-online.com/WL/VAA/GB/en", result.DeeplinkUrl);
            Assert.Equal(DeeplinkOption.TravelInsurance, result.DeeplinkOption);
        }

        [Fact]
        public async Task GetDeeplinkUrl_BaggageAllowance_ReturnsExpectedResponse()
        {
            // Arrange
            var deeplinkUrlQuery = _serviceProvider.GetService<DeeplinkUrlQuery>()!;
            var tripReservationId = await BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, "4d7e9512-f5d2-4572-84e1-b2fcdd773993");
            var request = new DeeplinkRequest
            {
                TripReservationId = tripReservationId,
                DeeplinkOption = DeeplinkOption.BaggageAllowance
            };

            // Act
            var result = await deeplinkUrlQuery.GetDeeplinkUrl(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://www.virginatlantic.com/help/categories/070fb328-0c1c-4a04-a2bf-795834008bf1", result.DeeplinkUrl);
            Assert.Equal(DeeplinkOption.BaggageAllowance, result.DeeplinkOption);
        }

        [Fact]
        public async Task GetDeeplinkUrl_FlightDisruption_ReturnsExpectedResponse()
        {
            // Arrange
            var deeplinkUrlQuery = _serviceProvider.GetService<DeeplinkUrlQuery>()!;
            var tripReservationId = await BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, "4d7e9512-f5d2-4572-84e1-b2fcdd773993");
            var request = new DeeplinkRequest
            {
                TripReservationId = tripReservationId,
                DeeplinkOption = DeeplinkOption.FlightDisruption
            };

            // Act
            var result = await deeplinkUrlQuery.GetDeeplinkUrl(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://www.virginatlantic.com/help/categories/dc2a02fb-d3fe-4e14-8a09-413bb96a8584", result.DeeplinkUrl);
            Assert.Equal(DeeplinkOption.FlightDisruption, result.DeeplinkOption);
        }

        [Fact]
        public async Task GetDeeplinkUrl_ManageHoliday_ReturnsExpectedResponse()
        {
            // Arrange
            var deeplinkUrlQuery = _serviceProvider.GetService<DeeplinkUrlQuery>()!;
            var request = new DeeplinkRequest
            {
                TripReservationId = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJtc0Jvb2tpbmdJZCI6IjY4Mzc2NDciLCJzdXJOYW1lIjoiSm9obnNvbiIsImJhc2tldElkIjoiNGQ2NjAxYTYtYjJjYS00ZGY2LTljZTgtMjU4NTQwZDViYTA4IiwiZG9iIjoiMTk3MC0wNC0xNSIsIm5iZiI6MTc4MDUxMjE4NiwiZXhwIjoxNzgwNTk4NTg2LCJpYXQiOjE3ODA1MTIxODZ9.v4jIFVtFh66FvPqfyjTPNHaikMz7yG_HxNA7tE9F3sU",               
                DeeplinkOption = DeeplinkOption.ManageHoliday
            };

            // Act
            var result = await deeplinkUrlQuery.GetDeeplinkUrl(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://www.virginatlantic.com/yourbooking", result.DeeplinkUrl);
            Assert.Equal(DeeplinkOption.ManageHoliday, result.DeeplinkOption);
        }

        [Fact]
        public async Task GetDeeplinkUrl_ConciergeTripAware_ReturnsExpectedResponse()
        {
            // Arrange
            var deeplinkUrlQuery = _serviceProvider.GetService<DeeplinkUrlQuery>()!;
            var request = new DeeplinkRequest
            {
                TripReservationId = "trip-reservation-id",
                DeeplinkOption = DeeplinkOption.ConciergeTripAware
            };

            // Act
            var result = await deeplinkUrlQuery.GetDeeplinkUrl(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://proxydeeplinktest.com/deeplink?id=concierge-trip-aware", result.DeeplinkUrl);
            Assert.Equal(DeeplinkOption.ConciergeTripAware, result.DeeplinkOption);
        }

        [Fact]
        public async Task GetDeeplinkUrl_ConciergeTripAware_MissingTripReservationId_Throws()
        {
            // Arrange
            var deeplinkUrlQuery = _serviceProvider.GetService<DeeplinkUrlQuery>()!;
            var request = new DeeplinkRequest
            {
                TripReservationId = string.Empty,
                DeeplinkOption = DeeplinkOption.ConciergeTripAware
            };

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => deeplinkUrlQuery.GetDeeplinkUrl(request));
        }

        [Fact]
        public async Task GetDeeplinkUrl_TravelInsurance_MissingCountry_Throws()
        {
            var deeplinkUrlQuery = _serviceProvider.GetService<DeeplinkUrlQuery>()!;
            var tripReservationId = await BookingIdHelper.CreateBookingId(null, "f93032f1-cc02-4edb-acc4-53af15e20ac7", "givenName", "surName", "ABCDEF", null, "4d7e9512-f5d2-4572-84e1-b2fcdd773992");

            // Arrange
            var request = new DeeplinkRequest
            {
                TripReservationId = tripReservationId,
                DeeplinkOption = DeeplinkOption.TravelInsurance
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => deeplinkUrlQuery.GetDeeplinkUrl(request));
        }

        [Fact]
        public async Task GetDeeplinkUrl_InvalidOption_ThrowsArgumentOutOfRange()
        {
            var deeplinkUrlQuery = _serviceProvider.GetService<DeeplinkUrlQuery>()!;
            var request = new DeeplinkRequest
            {
                TripReservationId = "ABC123",
                DeeplinkOption = (DeeplinkOption)999
            };

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => deeplinkUrlQuery.GetDeeplinkUrl(request));
        }

        [Fact]
        public async Task GetDeeplinkUrl_NullTripReservationId_Throws()
        {
            var deeplinkUrlQuery = _serviceProvider.GetService<DeeplinkUrlQuery>()!;
            var request = new DeeplinkRequest
            {
                TripReservationId = null,
                DeeplinkOption = DeeplinkOption.IROP_FAF
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() => deeplinkUrlQuery.GetDeeplinkUrl(request));
        }
    }
}
