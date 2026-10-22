using Microsoft.Extensions.Logging;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Trip.Application;
using VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Tests
{
    public class TripServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _logger;
        private readonly IAir4TokenGenerator _air4TokenGenerator;
        private readonly IDeltaTripsDetailsPrivateService _deltaTripsDetailsPrivateService;
        private readonly IDeltaTripsDetailsService _deltaTripsDetailsService;
        private readonly ILogger<TripService> _loggerTripService;

        public TripServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _air4TokenGenerator = new Air4TokenGenerator(_httpConnector);
            _deltaTripsDetailsPrivateService = new DeltaTripsDetailsPrivateService(_httpConnector, _air4TokenGenerator);
            _deltaTripsDetailsService = new DeltaTripsDetailsService(_httpConnector);
            _loggerTripService = Microsoft.Extensions.Logging.Abstractions.NullLogger<TripService>.Instance;

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            TripMSEnvironmentSetup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task TripsBooking_ShouldHaveAtLeastOneLeadPassenger_ShouldSucceed()
        {
            var tripservice = new TripService(_deltaTripsDetailsService, _deltaTripsDetailsPrivateService, _loggerTripService);

            // Arrange
            var tripRequest = new TripsDetailsRequest()
            {
                ConfirmationNum = "CIOXZO",
                Using = RequestOptionCodeType.PNR,
                GivenNames = "B",
                Surname = "YOSHIMURA",
            };

            var result = await tripservice.GetTripsDetails(tripRequest);

            Assert.NotNull(result);
            // Verify at least one passenger is marked as lead passenger
            Assert.True(result.Value?.Value?.Booking?.Passengers?.Any(p => p.IsLeadPassenger == true));
        }
    }
}
