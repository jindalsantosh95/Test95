using Newtonsoft.Json.Linq;
using HotChocolate;
using Xunit;
using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using GraphQLEnvironment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests
{
    [Collection("Historic Trips configuration")]
    public sealed class FindTripByFlightDateQueryTests : IDisposable
    {
        private readonly ServiceProvider _services;

        public FindTripByFlightDateQueryTests()
        {
            ConfigReader.Set(GraphQLEnvironment.OrderDataMSUrl, "https://Testresources.virginatlantic.com/dev/orderdata");
            ConfigReader.Set(GraphQLEnvironment.OrderDataOcpKey, "test-ocp-key");
            ConfigReader.Set(GraphQLEnvironment.OrderDataAppId, "test-app-id");
            ConfigReader.Set(HistoricTripsOAuthTokenProvider.TokenProviderUrlSetting, "https://testresources.virginatlantic.com/dev/oauth");
            var services = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory, VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory>()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider, HistoricTripsOAuthTokenProvider>();
            new Startup().ConfigureServices(services);
            var builder = services.AddGraphQL().AddQueryType(d => d.Name("Query"))
                .AddType<HotChocolate.Types.NodaTime.LocalDateTimeType>()
                .AddType<HotChocolate.Types.NodaTime.LocalDateType>()
                .AddType<HotChocolate.Types.NodaTime.LocalTimeType>();
            new Startup().RegisterQueries(builder, false);
            _services = services.BuildServiceProvider();
        }

        [Fact]
        public async Task PublicQuery_UsesTheExistingTripsBookingContract()
        {
            var executor = await _services.GetRequiredService<IRequestExecutorResolver>().GetRequestExecutorAsync();
            Assert.Equal("TripsBooking", executor.Schema.QueryType.Fields["FindTripByFlightDate"].Type.NamedType().Name);
            Assert.DoesNotContain(executor.Schema.QueryType.Fields, field => field.Name == "HistoricTripDetails");
            Assert.Contains("flightDate", executor.Schema.QueryType.Fields["FindTripByFlightDate"].Arguments.Select(argument => argument.Name));
            Assert.DoesNotContain("bookingDate", executor.Schema.QueryType.Fields["FindTripByFlightDate"].Arguments.Select(argument => argument.Name));
        }

        [Fact]
        public async Task PublicQuery_ReturnsNestedBookingWithoutStaffAuthorization()
        {
            var executor = await _services.GetRequiredService<IRequestExecutorResolver>().GetRequestExecutorAsync();
            var result = await executor.ExecuteAsync("""
                { FindTripByFlightDate(pnr: " abc123 ", flightDate: "31AUG25", givenName: " Jane ", surname: " Doe ") {
                    id
                    payment { __typename }
                    passengers { firstName lastName }
                    product {
                        passengers { firstName lastName }
                        reference { pnrLocator }
                        checkInEligible accessibleTravelEligible
                        slices { duration segments { duration marketingCarrier { flightNumber } origin { code } destination { code } } }
                    }
                } }
                """);

            var queryResult = Assert.IsAssignableFrom<IQueryResult>(result);
            Assert.Null(queryResult.Errors);
            Assert.Contains("ORDER-123", result.ToJson());
            Assert.Contains("Jane", result.ToJson());
            Assert.Contains("ABC123", result.ToJson());
            Assert.Contains("VS001", result.ToJson());
            var payload = Newtonsoft.Json.Linq.JObject.Parse(result.ToJson());
            Assert.Equal(new[] { "Jane", "John" }, payload.SelectTokens("data.FindTripByFlightDate[0].passengers[*].firstName").Select(token => token.Value<string>()));
            Assert.Equal(new[] { "Jane", "John" }, payload.SelectTokens("data.FindTripByFlightDate[0].product[0].passengers[*].firstName").Select(token => token.Value<string>()));
            Assert.Equal("PT5H", payload.SelectToken("data.FindTripByFlightDate[0].product[0].slices[0].duration")?.Value<string>());
            Assert.Equal("PT4H30M", payload.SelectToken("data.FindTripByFlightDate[0].product[0].slices[0].segments[0].duration")?.Value<string>());
            Assert.DoesNotContain("payment-only", result.ToJson());
            Assert.Contains("\"payment\": null", result.ToJson());
            var httpFactory = Assert.IsType<VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory>(
                _services.GetRequiredService<IHttpClientFactory>());
            var bookingResponse = Assert.Single(httpFactory.GetResponses("https://testresources.virginatlantic.com"),
                response => response.RequestMessage?.RequestUri?.AbsolutePath.EndsWith("/scv/orders/by-flight-date") == true);
            Assert.True(bookingResponse.RequestMessage?.Headers.CacheControl?.NoStore);
            Assert.Equal("TripsBooking", executor.Schema.QueryType.Fields["FindTripByFlightDate"].Type.NamedType().Name);
            Assert.DoesNotContain(executor.Schema.Types, type => type.Name == "ScvBooking" || type.Name == "ScvTicket");
            Assert.DoesNotContain("cancellationToken", executor.Schema.QueryType.Fields["FindTripByFlightDate"].Arguments.Select(argument => argument.Name));
        }

        [Fact]
        public async Task Lookup_StopsImmediatelyWhenAlreadyCancelled()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _services.GetRequiredService<IScvBookingProvider>()
                .LookupAsync("ABC123", "31AUG25", "Jane", "Doe", cancellation.Token));
        }

        [Fact]
        public async Task Lookup_CancelsDelayedDownstreamRequest()
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _services.GetRequiredService<IScvBookingProvider>()
                .LookupAsync("DELAY1", "31AUG25", "Jane", "Doe", cancellation.Token));
        }

        [Fact]
        public async Task Lookup_ReturnsEmptyListForUnknownPnr()
        {
            var provider = _services.GetRequiredService<IScvBookingProvider>();
            Assert.Empty(await provider.LookupAsync("MISSING", "31AUG25", "Jane", "Doe"));
        }

        [Fact]
        public async Task Lookup_ReturnsEmptyListForAmbiguousDownstreamResults()
        {
            var provider = _services.GetRequiredService<IScvBookingProvider>();
            Assert.Empty(await provider.LookupAsync("AMBIG1", "31AUG25", "Jane", "Doe"));
        }

        [Fact]
        public async Task Lookup_PropagatesDownstreamFailure()
        {
            var provider = _services.GetRequiredService<IScvBookingProvider>();
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => provider.LookupAsync("FAILED", "31AUG25", "Jane", "Doe"));
            Assert.Equal(HttpStatusCode.ServiceUnavailable, exception.StatusCode);
            Assert.Equal(true, exception.Data["Sensitive"]);
            Assert.Equal("[REDACTED]", exception.Data["Request"]);
            Assert.Equal("[REDACTED]", exception.Data["Response"]);
            Assert.Equal("[REDACTED]", await VAA.EAPI.GraphQL.Common.Extensions.HttpRequestExceptionExtensions.ReadResponseContentAsStringAsync(exception));
        }

        [Theory]
        [InlineData("31FEB25")]
        [InlineData("2025-08-31")]
        [InlineData(" ")]
        public async Task Lookup_RejectsInvalidFlightDate(string date)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _services.GetRequiredService<IScvBookingProvider>()
                .LookupAsync("ABC123", date, "Jane", "Doe"));
        }

        [Theory]
        [InlineData(" ", "Jane", "Doe")]
        [InlineData("ABC123", " ", "Doe")]
        [InlineData("ABC123", "Jane", " ")]
        public async Task Lookup_RejectsMissingCredentials(string pnr, string firstName, string surname)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _services.GetRequiredService<IScvBookingProvider>().LookupAsync(pnr, "31AUG25", firstName, surname));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public async Task Lookup_AcceptsPastTodayAndFutureFlightDates(int days)
        {
            var date = DateTime.UtcNow.Date.AddDays(days).ToString("ddMMMyy", System.Globalization.CultureInfo.InvariantCulture);
            var booking = Assert.Single(await _services.GetRequiredService<IScvBookingProvider>()
                .LookupAsync("ANYDAY", date, "Jane", "Doe"));
            Assert.Equal("ORDER-ANYDAY", booking.Id);
            var httpFactory = Assert.IsType<VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory>(
                _services.GetRequiredService<IHttpClientFactory>());
            Assert.Single(httpFactory.GetResponses("https://testresources.virginatlantic.com"),
                response => response.RequestMessage?.RequestUri?.AbsolutePath.EndsWith("/scv/orders/by-flight-date") == true);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public async Task Lookup_UsesExistingGroupBookingSetting(string setting, bool rejected)
        {
            var original = ConfigReader.Try(GraphQLEnvironment.ExcludeGroupBookings, true).ToString();
            try
            {
                ConfigReader.Set(GraphQLEnvironment.ExcludeGroupBookings, setting);
                var provider = _services.GetRequiredService<IScvBookingProvider>();
                if (rejected)
                {
                    var error = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LookupAsync("GROUP1", "31AUG25", "Jane", "Doe"));
                    Assert.Equal("Group bookings are not supported", error.Message);
                }
                else
                {
                    var booking = Assert.Single(await provider.LookupAsync("GROUP1", "31AUG25", "Jane", "Doe"));
                    Assert.Equal("group", booking.ReservationType);
                    Assert.Equal(2, booking.Passengers!.Count());
                    Assert.Equal(2, Assert.Single(booking.Product!).Passengers!.Count());
                }
            }
            finally
            {
                ConfigReader.Set(GraphQLEnvironment.ExcludeGroupBookings, original);
            }
        }

        public void Dispose() => _services.Dispose();
    }
}
