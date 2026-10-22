using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;
using VAA.EAPI.GraphQL.Domain.Trips.Queries;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Queries
{
    [Collection("Flight Ops configuration")]
    public class TripDetailsListQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IReferenceServiceProvider _mockReferenceServiceProvider;
        private readonly ITripsDetailsQueryProvider _mockTripsDetailsQueryProvider;
        private readonly ILogger<TripDetailsListQuery> _loggerTrip;
        private readonly HttpConnector _httpConnector;
        private readonly IOAuthTokenProvider _oAuthTokenProvider;
        private readonly ILogger<HttpConnector> _logger;
        private readonly TripDetailsListQuery _tripDetailsQuery;
        private readonly ITokenB2CProvider<string> _tokenB2Cprovider;
        private readonly IOrderDataProvider _orderDataProvider;
        private readonly IHolidayProvider _mockholidayProvider;
        private readonly ITripAircraftEligibilityService _tripAircraftEligibilityService;
        private readonly ITokenB2CProvider<string> _tokenProvider;
        private readonly TripDetailsListQuery _tripDetailsQueryWithFixedClock;

        public TripDetailsListQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerTrip = Microsoft.Extensions.Logging.Abstractions.NullLogger<TripDetailsListQuery>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _orderDataProvider = new OrderDataProvider(_httpConnector);
            _mockReferenceServiceProvider = new ReferenceServiceProvider(_httpConnector);
            _mockTripsDetailsQueryProvider = new TripsDetailsQueryProvider(_httpConnector);
            _mockholidayProvider = new HolidayProvider(_httpConnector);
            _tripAircraftEligibilityService = new TripAircraftEligibilityService(new OperationalScheduleProvider(_httpConnector), new StarlinkEligibilityProvider(_httpConnector), Microsoft.Extensions.Logging.Abstractions.NullLogger<TripAircraftEligibilityService>.Instance);

            var identityMsResponseJson = @"{
                ""sub"": ""1234567890"",
                ""name"": ""John Doe"",
                ""email"": ""johndoe@example.com"",
                ""roles"": [""User""],
                ""azp"": ""my-client-id"",
                ""iss"": ""https://my-tenant-name.b2clogin.com/my-tenant-name.onmicrosoft.com/v2.0/"",
                ""fcnumber"": ""1234567890""
            }";

            var mockB2C = new Mock<ITokenB2CProvider<string>>();
            mockB2C.Setup(m => m.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(identityMsResponseJson);
            _tokenProvider = mockB2C.Object;

            _tripDetailsQuery = new TripDetailsListQuery(_mockReferenceServiceProvider, _mockTripsDetailsQueryProvider, _mockholidayProvider, _loggerTrip, mockB2C.Object, _orderDataProvider, _tripAircraftEligibilityService);
            var fixedClockService = new TripAircraftEligibilityService(
                new OperationalScheduleProvider(_httpConnector),
                new StarlinkEligibilityProvider(_httpConnector),
                Microsoft.Extensions.Logging.Abstractions.NullLogger<TripAircraftEligibilityService>.Instance,
                new FixedDateTimeProvider(new DateTime(2026, 10, 27, 0, 0, 0, DateTimeKind.Utc)));
            _tripDetailsQueryWithFixedClock = new TripDetailsListQuery(_mockReferenceServiceProvider, _mockTripsDetailsQueryProvider, _mockholidayProvider, _loggerTrip, _tokenProvider, _orderDataProvider, fixedClockService);
        }

        [Fact]
        public async Task TripDetails_VsPremiumBooking_ReturnsPotentialUpgradeAvailable()
        {
            var result = await _tripDetailsQueryWithFixedClock.GetTripDetailsList(new TripDetailsRequest
            {
                TripDetails = [new() { PnrLocator = "C4F2RI", Surname = "Clarke", GivenNames = "C" }]
            });

            var booking = Assert.Single(result);
            var segments = booking.Product!
                .SelectMany(product => product.Slices ?? [])
                .SelectMany(slice => slice.Segments ?? [])
                .ToArray();

            Assert.NotEmpty(segments);
            Assert.All(segments, segment => Assert.True(segment.PotentialUpgradeAvailable));
            Assert.True(booking.PotentialUpgradeAvailable);
        }

        [Fact]
        public async Task TripDetails_NonVirginAtlanticBooking_ReturnsPotentialUpgradeUnavailable()
        {
            var result = await _tripDetailsQuery.GetTripDetailsList(new TripDetailsRequest
            {
                TripDetails =
                [
                    new()
                    {
                        BookingReference = "9999998",
                        DateOfBirth = DateOnly.Parse("1970-01-01"),
                        Surname = "User"
                    }
                ]
            });

            var booking = Assert.Single(result);
            var segment = booking.Product!
                .Single(product => product.BookingType == "AIRLINE")
                .Slices!
                .SelectMany(slice => slice.Segments ?? [])
                .Single();

            Assert.False(segment.PotentialUpgradeAvailable);
            Assert.False(booking.PotentialUpgradeAvailable);
        }

        [Fact]
        public async Task TripDetails_MixedCarrierSegments_ReturnsSegmentEligibilityAndBookingAvailability()
        {
            var result = await _tripDetailsQuery.GetTripDetailsList(new TripDetailsRequest
            {
                TripDetails = [new() { PnrLocator = "DNC6NL", Surname = "surName", GivenNames = "givenName" }]
            });

            var booking = Assert.Single(result);
            var segments = booking.Product!
                .SelectMany(product => product.Slices ?? [])
                .SelectMany(slice => slice.Segments ?? [])
                .ToArray();

            Assert.Contains(segments, segment => segment.PotentialUpgradeAvailable);
            Assert.Contains(segments, segment => !segment.PotentialUpgradeAvailable);
            Assert.True(booking.PotentialUpgradeAvailable);
        }

        [Fact]
        public async Task TripDetails_EligibleVsLeg_PopulatesAircraftTailNumberAndStarlinkEligibility()
        {
            var result = await _tripDetailsQueryWithFixedClock.GetTripDetailsList(new TripDetailsRequest
            {
                TripDetails = [new() { PnrLocator = "C4F2RI", Surname = "Clarke", GivenNames = "C" }]
            });

            var legs = result
                .SelectMany(booking => booking.Product ?? [])
                .SelectMany(product => product.Slices ?? [])
                .SelectMany(slice => slice.Segments ?? [])
                .SelectMany(segment => segment.Legs ?? [])
                .ToArray();

            var leg = legs.FirstOrDefault(x => x.Aircraft?.TailNumber == "G-VTOM");
            Assert.NotNull(leg);

            Assert.Equal("G-VTOM", leg!.Aircraft?.TailNumber);
            Assert.True(leg.IsStarlinkEligible);
        }

        [Fact]
        public async Task TripDetails_SupportedHolidayPnr_PopulatesStarlinkEligibilityOnFlightProduct()
        {
            var result = await _tripDetailsQueryWithFixedClock.GetTripDetailsList(new TripDetailsRequest
            {
                TripDetails =
                [
                    new()
                    {
                        BookingReference = "6996219",
                        DateOfBirth = new DateOnly(1988, 11, 29),
                        Surname = "Walker"
                    }
                ]
            });

            var holidayBooking = Assert.Single(result);
            var flightProduct = Assert.Single(holidayBooking.Product!.Where(product => product.SupportedFlight));
            var leg = flightProduct.Slices!
                .SelectMany(slice => slice.Segments ?? [])
                .SelectMany(segment => segment.Legs ?? [])
                .First(leg => leg.Aircraft?.TailNumber == "G-VTOM");

            Assert.Equal("ETNWM4", flightProduct.Reference?.PnrLocator);
            Assert.Equal("G-VTOM", leg.Aircraft?.TailNumber);
            Assert.True(leg.IsStarlinkEligible);
        }

        [Fact]
        public async Task TripDetails_StarlinkReturnsFalse_PopulatesFalseEligibility()
        {
            ConfigReader.Set(Common.Constants.Environment.FlightOpsUrl, "http://tempURL.org/starlink-false");

            var result = await _tripDetailsQueryWithFixedClock.GetTripDetailsList(new TripDetailsRequest
            {
                TripDetails = [new() { PnrLocator = "C4F2RI", Surname = "Clarke", GivenNames = "C" }]
            });

            var legs = result.SelectMany(booking => booking.Product ?? [])
                .SelectMany(product => product.Slices ?? [])
                .SelectMany(slice => slice.Segments ?? [])
                .SelectMany(segment => segment.Legs ?? []);
            var leg = legs.First(x => x.Aircraft?.TailNumber == "G-VTOM");

            Assert.False(leg.IsStarlinkEligible);
        }

        [Fact]
        public async Task TripDetails_StarlinkFails_ReturnsNullEligibility()
        {
            ConfigReader.Set(Common.Constants.Environment.FlightOpsUrl, "http://tempURL.org/starlink-error");

            var result = await _tripDetailsQueryWithFixedClock.GetTripDetailsList(new TripDetailsRequest
            {
                TripDetails = [new() { PnrLocator = "C4F2RI", Surname = "Clarke", GivenNames = "C" }]
            });

            var legs = result.SelectMany(booking => booking.Product ?? [])
                .SelectMany(product => product.Slices ?? [])
                .SelectMany(slice => slice.Segments ?? [])
                .SelectMany(segment => segment.Legs ?? []);
            var leg = legs.First(x => x.Aircraft?.TailNumber == "G-VTOM");

            Assert.Null(leg.IsStarlinkEligible);
        }

        [Fact]
        public async Task TripDetails_EligibilityServiceFailures_DoNotImpactBooking()
        {
            ConfigReader.Set(Common.Constants.Environment.FlightOpsUrl, "http://tempURL.org/error");

            var operationalScheduleFailure = await _tripDetailsQueryWithFixedClock.GetTripDetailsList(new TripDetailsRequest
            {
                TripDetails = [new() { PnrLocator = "C4F2RI", Surname = "Clarke", GivenNames = "C" }]
            });

            var bookingAfterOperationalScheduleFailure = Assert.Single(operationalScheduleFailure);
            Assert.NotEmpty(bookingAfterOperationalScheduleFailure.Product ?? []);

            ConfigReader.Set(Common.Constants.Environment.FlightOpsUrl, "http://tempURL.org/starlink-error");

            var starlinkFailure = await _tripDetailsQueryWithFixedClock.GetTripDetailsList(new TripDetailsRequest
            {
                TripDetails = [new() { PnrLocator = "C4F2RI", Surname = "Clarke", GivenNames = "C" }]
            });

            var bookingAfterStarlinkFailure = Assert.Single(starlinkFailure);
            Assert.NotEmpty(bookingAfterStarlinkFailure.Product ?? []);

            var leg = bookingAfterStarlinkFailure.Product!
                .SelectMany(product => product.Slices ?? [])
                .SelectMany(slice => slice.Segments ?? [])
                .SelectMany(segment => segment.Legs ?? [])
                .First(leg => leg.Aircraft?.TailNumber == "G-VTOM");

            Assert.Null(leg.IsStarlinkEligible);
        }

        private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
        {
            public DateTime Now => utcNow.ToLocalTime();
            public DateTime UtcNow => utcNow;
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenDependenciesAreNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TripDetailsListQuery(null, null, null, null, null, null, null));
        }

        [Fact]
        public async Task TripDetails_Having4xx5xx2xx_ShouldReturn2xx5xxBookings()
        {
            // Arrange
            var rawJwt = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IkFCQ0RFRjEyMzQ1In0.eyJhdWQiOiIxMTExMTExMS0yMjIyLTMzMzMtNDQ0NC01NTU1NTU1NTU1NTUiLCJpc3MiOiJodHRwczovL2NvbnRvc28uYjJjbG9naW4uY29tL2NvbnRvc28ub25taWNyb3NvZnQuY29tL3YyLjAvIiwiaWF0IjoxNzEwMDAwMDAwLCJleHAiOjE3MTAwMDM2MDAsInRmcCI6IkIyQ18xX3NpZ251cHNpZ25pbiIsInN1YiI6ImFhYWFhYWFhLWJiYmItY2NjYy1kZGRkLWVlZWVlZWVlZWVlIiwic2NwIjoicmVhZCIsImVtYWlscyI6WyJ1c2VyQGNvbnRvc28uY29tIl19.QW5Zc2lnbmF0dXJlVmFsdWU";

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(rawJwt);
            var identity = new ClaimsIdentity(jwt.Claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var authToken = new AuthToken { Token = rawJwt };
            AsyncContext.Current.Context.Features.Set(new JwtPrincipalFeature(principal, authToken));

            var request = new TripDetailsRequest()
            {
                TripDetails =
                [
                  new()
                  {
                     PnrLocator = "ELFI8L",
                     Surname = "S",
                     GivenNames = "Winson",
                     CreatedDate = DateTime.Parse("2025-10-15T00:00:00.000Z")
                  },
                   new()
                  {
                     PnrLocator = "AHDHDJ",
                     Surname = "S",
                     GivenNames = "Winson",
                     CreatedDate = DateTime.Parse("2025-11-25T00:00:00.000Z")
                  },
                   new()
                  {
                     PnrLocator = "ELFI8K",
                     Surname = "S",
                     GivenNames = "#$%^&*()_VNJMK<GHJK",
                     CreatedDate = DateTime.Parse("2025-11-25T00:00:00.000Z")
                  },
                    new()
                  {
                     PnrLocator = "EB9XBI",
                     Surname = "T",
                     GivenNames = "Testing",
                     CreatedDate = DateTime.Parse("2025-11-25T00:00:00.000Z")
                  },
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("ELFI8K", result.Select(r => r?.TripSearchRequest?.PnrLocator));
            Assert.DoesNotContain("EB9XBI", result.Select(r => r?.TripSearchRequest?.PnrLocator));
            Assert.Contains("ELFI8L", result.Select(r => r?.TripSearchRequest?.PnrLocator));
            Assert.Contains("AHDHDJ", result.Select(r => r?.TripSearchRequest?.PnrLocator));
            Assert.True(result.Any(c => c.TripSearchRequest.PnrLocator == "EA7BPA" && c.IsAccountLinkedPNR == true));

        }

        [Fact]
        public async Task TripDetails_HavingIROP_ShouldReturnBookingIdAlways()
        {
            // Arrange
            var rawJwt = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IkFCQ0RFRjEyMzQ1In0.eyJhdWQiOiIxMTExMTExMS0yMjIyLTMzMzMtNDQ0NC01NTU1NTU1NTU1NTUiLCJpc3MiOiJodHRwczovL2NvbnRvc28uYjJjbG9naW4uY29tL2NvbnRvc28ub25taWNyb3NvZnQuY29tL3YyLjAvIiwiaWF0IjoxNzEwMDAwMDAwLCJleHAiOjE3MTAwMDM2MDAsInRmcCI6IkIyQ18xX3NpZ251cHNpZ25pbiIsInN1YiI6ImFhYWFhYWFhLWJiYmItY2NjYy1kZGRkLWVlZWVlZWVlZWVlIiwic2NwIjoicmVhZCIsImVtYWlscyI6WyJ1c2VyQGNvbnRvc28uY29tIl19.QW5Zc2lnbmF0dXJlVmFsdWU";

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(rawJwt);
            var identity = new ClaimsIdentity(jwt.Claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var authToken = new AuthToken { Token = rawJwt };
            AsyncContext.Current.Context.Features.Set(new JwtPrincipalFeature(principal, authToken));

            var request = new TripDetailsRequest()
            {
                TripDetails =
                [
                  new()
                  {
                     PnrLocator = "C58TMB",
                     Surname = "S",
                     GivenNames = "Winson",
                     CreatedDate = DateTime.Parse("2025-10-15T00:00:00.000Z")
                  }
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.All(b => b.Passengers?.Any() == true));
            Assert.True(result.All(b => b.Product?.Any() == true));
            Assert.Contains(result, b => !b.Id.IsNullOrEmpty());

            // excludes group bookings, so only the non-group booking should be returned even if there are multiple bookings linked to the PNR
            var booking = Assert.Single(result);
            Assert.Equal("C58TMB", booking.Product.First().Reference.PnrLocator);
            Assert.DoesNotContain(result, b => b.Product.First().Reference.PnrLocator == "ELFI8M");
        }

        [Fact]
        public async Task TripDetails_IROP_Cleaned_ShouldPreserveCheckInEligibility()
        {
            // Arrange
            var rawJwt = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IkFCQ0RFRjEyMzQ1In0.eyJhdWQiOiIxMTExMTExMS0yMjIyLTMzMzMtNDQ0NC01NTU1NTU1NTU1NTUiLCJpc3MiOiJodHRwczovL2NvbnRvc28uYjJjbG9naW4uY29tL2NvbnRvc28ub25taWNyb3NvZnQuY29tL3YyLjAvIiwiaWF0IjoxNzEwMDAwMDAwLCJleHAiOjE3MTAwMDM2MDAsInRmcCI6IkIyQ18xX3NpZ251cHNpZ25pbiIsInN1YiI6ImFhYWFhYWFhLWJiYmItY2NjYy1kZGRkLWVlZWVlZWVlZWVlIiwic2NwIjoicmVhZCIsImVtYWlscyI6WyJ1c2VyQGNvbnRvc28uY29tIl19.QW5Zc2lnbmF0dXJlVmFsdWU";

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(rawJwt);
            var identity = new ClaimsIdentity(jwt.Claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var authToken = new AuthToken { Token = rawJwt };
            AsyncContext.Current.Context.Features.Set(new JwtPrincipalFeature(principal, authToken));

            var request = new TripDetailsRequest()
            {
                TripDetails =
                [
                  new()
                  {
                     PnrLocator = "C58TMB",
                     Surname = "S",
                     GivenNames = "Winson",
                     CreatedDate = DateTime.Parse("2025-10-15T00:00:00.000Z")
                  }
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            var booking = Assert.Single(result);
            Assert.True(booking.Product.First().CheckInEligible);
            Assert.True(booking.Product.First().Slices.First().CheckInEligible);
        }

        [Fact]
        public async Task TripDetails_HavingIROP_ShouldReturnBookingIdAlways_IncludingGroups()
        {
            ConfigReader.Set("TripSettings:ExcludeGroupBookings", bool.FalseString);

            // Arrange
            var rawJwt = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IkFCQ0RFRjEyMzQ1In0.eyJhdWQiOiIxMTExMTExMS0yMjIyLTMzMzMtNDQ0NC01NTU1NTU1NTU1NTUiLCJpc3MiOiJodHRwczovL2NvbnRvc28uYjJjbG9naW4uY29tL2NvbnRvc28ub25taWNyb3NvZnQuY29tL3YyLjAvIiwiaWF0IjoxNzEwMDAwMDAwLCJleHAiOjE3MTAwMDM2MDAsInRmcCI6IkIyQ18xX3NpZ251cHNpZ25pbiIsInN1YiI6ImFhYWFhYWFhLWJiYmItY2NjYy1kZGRkLWVlZWVlZWVlZWVlIiwic2NwIjoicmVhZCIsImVtYWlscyI6WyJ1c2VyQGNvbnRvc28uY29tIl19.QW5Zc2lnbmF0dXJlVmFsdWU";

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(rawJwt);
            var identity = new ClaimsIdentity(jwt.Claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var authToken = new AuthToken { Token = rawJwt };
            AsyncContext.Current.Context.Features.Set(new JwtPrincipalFeature(principal, authToken));

            var request = new TripDetailsRequest()
            {
                TripDetails =
                [
                  new()
                  {
                     PnrLocator = "C58TMB",
                     Surname = "S",
                     GivenNames = "Winson",
                     CreatedDate = DateTime.Parse("2025-10-15T00:00:00.000Z")
                  }
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            Assert.NotNull(result);

            Assert.True(result.All(b => b.Passengers?.Any() == true));
            Assert.True(result.All(b => b.Product?.Any() == true));
            Assert.Contains(result, b => !b.Id.IsNullOrEmpty());

            // includes group bookings, so multiple bookings linked to the PNR should be returned
            Assert.Equal(2, result.Count());
            Assert.Contains(result, b => b.Product.First().Reference.PnrLocator == "ELFI8M");
        }

        [Fact]
        public async Task TripDetails_ShouldReturnBookings_WithAirlineReferenceData()
        {
            // Arrange
            var request = new TripDetailsRequest()
            {
                TripDetails =
                [
                  new()
                  {
                     PnrLocator = "FD2D95",
                     Surname = "KOPACZ",
                     GivenNames = "ANGELIC",
                     CreatedDate = DateTime.Parse("2025-10-15T00:00:00.000Z")
                  }
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            Assert.NotNull(result);
	            Assert.True(result.All(b => b.Passengers?.Any() == true));
	            Assert.True(result.All(b => b.Product?.Any() == true));
	            Assert.Equal(request.TripDetails.Count(), result.Count());
	            Assert.Equal("Virgin Atlantic For Test", result.First().Product.First().Slices.First().Segments.First().OperatingCarrier.CarrierName);
	            var segment = result.First().Product.First().Slices.First().Segments.First();
	            Assert.Equal(new DateTime(2025, 12, 6, 8, 55, 0), segment.PopsOpenDateTimeUtc);
	            Assert.Equal(new DateTime(2025, 12, 12, 8, 55, 0), segment.PopsCloseDateTimeUtc);
	            Assert.Null(segment.OtherAirlineOperatedHelpTitle);
	            Assert.Null(segment.OtherAirlineOperatedHelpText);
	        }

	        [Fact]
	        public async Task TripDetails_ShouldPopulateAirlineName_FromReferenceData_ForUnsupportedHolidayFlights()
	        {
	            var request = new TripDetailsRequest()
	            {
	                TripDetails =
	                [
	                  new()
	                  {
	                     BookingReference = "9999998",
	                     DateOfBirth = DateOnly.Parse("1970-01-01"),
	                     Surname = "User"
	                  }
	                ]
	            };

	            var result = await _tripDetailsQuery.GetTripDetailsList(request);

	            var booking = Assert.Single(result);
	            Assert.NotNull(booking.Product);
	
	            var airlineProduct = booking.Product.First(p => p.BookingType == "AIRLINE");
	            Assert.False(airlineProduct.SupportedFlight);
	            Assert.NotNull(airlineProduct.Slices);
	
	            var slice = airlineProduct.Slices.First();
	            Assert.NotNull(slice.Segments);
	            Assert.Equal("Dubai", slice.Destination?.CityName);
	
	            var segment = slice.Segments.First();
	            Assert.NotNull(segment.OperatingCarrier);
	            Assert.NotNull(segment.MarketingCarrier);
	
	            Assert.Equal("EK", segment.OperatingCarrier.CarrierCode);
	            Assert.Equal("EK", segment.MarketingCarrier.CarrierCode);
	            Assert.Equal("Emirates Airline", segment.OperatingCarrier.CarrierName);
	            Assert.Equal("Emirates Airline", segment.MarketingCarrier.CarrierName);	            
	            Assert.Equal("This flight is with Emirates Airline", segment.OtherAirlineOperatedHelpTitle);
	            Assert.Equal(ConfigReader.Get(Constants.Environment.OtherAirlineOperatedHelpText), segment.OtherAirlineOperatedHelpText);
	            Assert.Null(segment.PopsOpenDateTimeUtc);
	            Assert.Null(segment.PopsCloseDateTimeUtc);
	        }

	        [Fact]
	        public async Task TripDetails_ShouldPopulateKoreanAirHelp_ForOtherAirlineFlight()
	        {
	            var request = new TripDetailsRequest
	            {
	                TripDetails =
	                [
	                    new()
	                    {
	                        PnrLocator = "ABCKE1",
	                        Surname = "surName",
	                        GivenNames = "givenName"
	                    }
	                ]
	            };

	            var result = await _tripDetailsQuery.GetTripDetailsList(request);

	            var segment = Assert.Single(result).Product.First().Slices.First().Segments.First();
            Assert.Equal("This flight is with Korean Air", segment.OtherAirlineOperatedHelpTitle);
            Assert.Equal(ConfigReader.Get(Constants.Environment.KoreanAirlineOperatedHelpText), segment.OtherAirlineOperatedHelpText);
            Assert.Null(segment.PopsOpenDateTimeUtc);
            Assert.Null(segment.PopsCloseDateTimeUtc);
	        }


	        [Fact]
	        public async Task TripDetails_ShouldNotReturn_ExpiredHolidaysBookings()
	        {
            // Arrange
            var request = new TripDetailsRequest()
            {
                TripDetails =
                [
                  new()
                  {
                     BookingReference = "6999383",
                     DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                     Surname = "TEST"
                  },
                  new()
                  {
                     BookingReference = "6661909",
                     DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                     Surname = "TEST"
                  }
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task TripDetails_ShouldReturnFlightandHolidayBookingsResposne_ForMixedRequest()
        {
            // Arrange
            var request = new TripDetailsRequest()
            {
                TripDetails =
                [
                  new()
                  {
                    CreatedDate = null,
                    FlyingClubNumber = null,
                    PnrLocator = "EK49YD",
                    GivenNames = "BARBRA",
                    Surname = "STARY"
                  },
                  new()
                  {
                    CreatedDate = null,
                    FlyingClubNumber = null,
                    PnrLocator = "EK2CGQ",
                    GivenNames = "LOU",
                    Surname = "SCHURZ"
                  },
                  new()
                  {
                    BookingReference = "7001819",
                    DateOfBirth = DateOnly.Parse("1979-08-22"),
                    GivenNames = null,
                    Surname = "Dawson"
                  },
                  new()
                  {
                    BookingReference = "7000858",
                    DateOfBirth = DateOnly.Parse("1981-06-16"),
                    GivenNames = null,
                    Surname = "Ayres"
                  },
                  new()
                  {
                    BookingReference = "6997207",
                    DateOfBirth = DateOnly.Parse("1996-01-15"),
                    GivenNames = null,
                    Surname = "Rogerson"
                  }
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, b => b?.TripSearchRequest?.BookingReference == "6997207");
            Assert.Contains(result, b => b?.TripSearchRequest?.BookingReference == "7001819");
            Assert.Contains(result, b => b?.TripSearchRequest?.BookingReference == "7000858");
            Assert.Equal(request.TripDetails.Count(), result.Count());
            Assert.IsType<IEnumerable<Booking>>(result, exactMatch: false);
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            //flight trip response
            var tripResponse = result?.First(r => r?.TripSearchRequest?.PnrLocator == "EK2CGQ");
            Assert.NotNull(tripResponse);
            Assert.True(tripResponse.Passengers?.Any() == true);
            Assert.Equal(1, tripResponse.Product?.Count());
            Assert.Equal("AIRLINE", tripResponse.Product.FirstOrDefault().BookingType);
            Assert.NotNull(tripResponse.Product.FirstOrDefault().Slices);
            Assert.False(string.IsNullOrWhiteSpace(tripResponse.Id));
            Assert.True(jwtSecurityTokenHandler.CanReadToken(tripResponse.Id));

            var unknownStatusSlice = tripResponse.Passengers.Single().Slices.First();
            Assert.Null(unknownStatusSlice.AreTravelDocsComplete);
            Assert.Empty(unknownStatusSlice.TravelDocsStatusSummary);

            var multiPassengerTrip = result.First(r => r?.TripSearchRequest?.PnrLocator == "EK49YD");
            var barbraSlice = multiPassengerTrip.Passengers.Single(passenger => passenger.FirstName == "BARBRA").Slices.Single();
            var vivanSlice = multiPassengerTrip.Passengers.Single(passenger => passenger.FirstName == "VIVAN").Slices.Single();
            Assert.Null(barbraSlice.AreTravelDocsComplete);
            Assert.Empty(barbraSlice.TravelDocsStatusSummary);
            Assert.Null(vivanSlice.AreTravelDocsComplete);
            Assert.Empty(vivanSlice.TravelDocsStatusSummary);

            //holiday trip response
            var holidayResponse = result?.FirstOrDefault(x => x.TripSearchRequest?.BookingReference == "7000858");
            Assert.NotNull(holidayResponse);
            Assert.IsType<Booking>(holidayResponse);
            Assert.Equal(4, holidayResponse?.Product?.Count());
            Assert.DoesNotContain(holidayResponse.Product!, p => p.ExtraDetails?.ElementGroup is { } eg && Constants.Environment.HiddenElementGroups.Contains(eg));
            Assert.False(string.IsNullOrWhiteSpace(holidayResponse.TripResourceId));
            Assert.True(jwtSecurityTokenHandler.CanReadToken(holidayResponse.TripResourceId));

            Assert.True(holidayResponse?.Product?.Any(x => x.BookingType == "AIRLINE") == true);
            var airlineProduct = holidayResponse?.Product?.First(x => x.BookingType == "AIRLINE");
            Assert.NotNull(airlineProduct?.Slices);
            Assert.NotNull(airlineProduct?.Passengers);
            Assert.True(holidayResponse?.Product?.Any(x => x.BookingType == "CAR") == true);
            var carProduct = holidayResponse?.Product?.First(x => x.BookingType == "CAR");
            Assert.NotNull(carProduct?.CarDetails);
        }

        [Fact]
        public async Task TripDetails_ShouldOnlyReturnHolidayBookingsResponse_ForHolidayRequest()
        {
            // Arrange
            var request = new TripDetailsRequest()
            {
                TripDetails =
                [
                  new()
                  {
                    BookingReference = "6997207",
                    DateOfBirth = DateOnly.Parse("1996-01-15"),
                    GivenNames = null,
                    Surname = "Rogerson"
                  },
                  new()
                  {
                    BookingReference = "7001819",
                    DateOfBirth = DateOnly.Parse("1979-08-22"),
                    GivenNames = null,
                    Surname = "Dawson"
                  },
                  new()
                  {
                    BookingReference = "7000858",
                    DateOfBirth = DateOnly.Parse("1981-06-16"),
                    GivenNames = null,
                    Surname = "Ayres"
                  }
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, b => b?.TripSearchRequest?.BookingReference == "6997207");
            Assert.Contains(result, b => b?.TripSearchRequest?.BookingReference == "7001819");
            Assert.Contains(result, b => b?.TripSearchRequest?.BookingReference == "7000858");
            Assert.Equal(request.TripDetails.Count(), result.Count());
            Assert.IsType<IEnumerable<Booking>>(result, exactMatch: false);
            Assert.All(result.FirstOrDefault()?.Product, p => Assert.Contains(p.BookingType, new[] { "AIRLINE", "HOTEL" }));
            Assert.All(result.Skip(1).FirstOrDefault()?.Product, p => Assert.Contains(p.BookingType, new[] { "AIRLINE", "CAR", "EXTRA" }));
            Assert.All(result.Skip(2).FirstOrDefault()?.Product, p => Assert.Contains(p.BookingType, new[] { "AIRLINE", "EXTRA" }));
            Assert.True(result.All(r => r?.Product?.Count() > 1));
        }

        [Fact]
        public async Task TripDetails_ShouldReturnFlightEvenIfHolidayBookingsResponseFails_ForMixedRequest()
        {
            // Arrange
            var request = new TripDetailsRequest()
            {
                TripDetails =
                [
                  new()
                  {
                    CreatedDate = null,
                    FlyingClubNumber = null,
                    PnrLocator = "EK49YD",
                    GivenNames = "BARBRA",
                    Surname = "STARY"
                  },
                  new()
                  {
                    CreatedDate = null,
                    FlyingClubNumber = null,
                    PnrLocator = "EK2CGQ",
                    GivenNames = "LOU",
                    Surname = "SCHURZ"
                  },
                  new()
                  {
                    BookingReference = "6661908",
                    DateOfBirth = DateOnly.Parse("1979-08-22"),
                    GivenNames = null,
                    Surname = "Dawson"
                  },
                  new()
                  {
                    BookingReference = "6999381",
                    DateOfBirth = DateOnly.Parse("1979-08-22"),
                    GivenNames = null,
                    Surname = "TEST"
                  }
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.TripDetails.Count(), result.Count());
            Assert.IsType<IEnumerable<Booking>>(result, exactMatch: false);

            //flight trip response
            var tripResponse = result?.First(r => r?.TripSearchRequest?.PnrLocator == "EK2CGQ");
            Assert.NotNull(tripResponse);
            Assert.True(tripResponse.Passengers?.Any() == true);
            Assert.Equal(1, tripResponse.Product?.Count());
            Assert.Equal("AIRLINE", tripResponse.Product.FirstOrDefault().BookingType);
            Assert.NotNull(tripResponse.Product.FirstOrDefault().Slices);

            //holiday trip response
            var holidayResponse = result?.FirstOrDefault(x => x.TripSearchRequest?.BookingReference == "6661908");
            Assert.NotNull(holidayResponse);

        }

        [Fact]
        public async Task TripDetails_ShouldReturnAllErrorsOfHolidaysBooking()
        {
            // Arrange
            var request = new TripDetailsRequest()
            {
                TripDetails =
                [

                  new()
                  {
                    BookingReference = "1212121",
                    DateOfBirth = DateOnly.Parse("1979-08-22"),
                    GivenNames = null,
                    Surname = "Dawson"
                  },
                  new()
                  {
                    BookingReference = "2323232",
                    DateOfBirth = DateOnly.Parse("1979-08-22"),
                    GivenNames = null,
                    Surname = "TEST"
                  }
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.TripDetails.Count(), result.Count());
            Assert.IsType<IEnumerable<Booking>>(result, exactMatch: false);

            var bookingTboxError = result.First(x => x.TripSearchRequest.BookingReference == "1212121");
            var bookingCruiseError = result.First(x => x.TripSearchRequest.BookingReference == "2323232");

            Assert.Single(bookingTboxError.ResponseStatus.Errors);
            Assert.Single(bookingCruiseError.ResponseStatus.Errors);

            Assert.Equal("HOL007", bookingTboxError.ResponseStatus.Errors.First().Code);
            Assert.Equal("HOL001", bookingCruiseError.ResponseStatus.Errors.First().Code);
        }

        [Fact]
        public async Task TripDetails_ShouldReturnOneError_OneSuccessOfHolidaysBooking()
        {
            // Arrange
            var request = new TripDetailsRequest()
            {
                TripDetails =
                [

                  new()
                  {
                    BookingReference = "4545454",
                    DateOfBirth = DateOnly.Parse("1979-08-22"),
                    GivenNames = null,
                    Surname = "Dawson"
                  },
                  new()
                  {
                    BookingReference = "5656565",
                    DateOfBirth = DateOnly.Parse("1979-08-22"),
                    GivenNames = null,
                    Surname = "TEST"
                  }
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.TripDetails.Count(), result.Count());
            Assert.IsType<IEnumerable<Booking>>(result, exactMatch: false);

            var bookingTboxError = result.First(x => x.TripSearchRequest.BookingReference == "4545454");
            var successBooking = result.First(x => x.TripSearchRequest.BookingReference == "5656565");

            Assert.Single(bookingTboxError.ResponseStatus.Errors);
            Assert.Null(successBooking.ResponseStatus.Errors);

            Assert.Equal("HOL007", bookingTboxError.ResponseStatus.Errors.First().Code);
            Assert.Equal("5656565", successBooking.Id);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, bookingTboxError.ResponseStatus.Status);
            Assert.Equal(System.Net.HttpStatusCode.OK, successBooking.ResponseStatus.Status);

        }

        [Fact]
        public async Task TripQuery_ReturnLeadPassengerAlways_ReturnsSuccess()
        {
            // Arrange
            var request = new TripDetailsRequest()
            {
                TripDetails =
                [
                  new()
                  {
                    PnrLocator = "DNC6NL",
                    GivenNames = "givenName",
                    Surname = "surName"
                  }
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            Assert.NotNull(result);

            // Make sure passenger list always has at least one lead passenger
            var tripResponse = result.First(x => x.TripSearchRequest.PnrLocator == "DNC6NL");
            Assert.True(tripResponse.Passengers?.Any());
            Assert.True(tripResponse.Passengers?.Any(p => p.LeadPassenger == true));
            Assert.Equal("PT10H45M", tripResponse.Product?.First().Slices?.First().Duration);
        }

        [Fact]
        public async Task TripDetails_ShouldReturnLeadPassengerForVSSupportedFlights_ForHolidayRequest()
        {
            // Arrange
            var request = new TripDetailsRequest()
            {
                TripDetails =
                [
                  new()
                  {
                    BookingReference = "6997207",
                    DateOfBirth = DateOnly.Parse("1996-01-15"),
                    GivenNames = null,
                    Surname = "Rogerson"
                  },
                  new()
                  {
                    BookingReference = "7001819",
                    DateOfBirth = DateOnly.Parse("1979-08-22"),
                    GivenNames = null,
                    Surname = "Dawson"
                  },
                  new()
                  {
                    BookingReference = "7000858",
                    DateOfBirth = DateOnly.Parse("1981-06-16"),
                    GivenNames = null,
                    Surname = "Ayres"
                  }
                ]
            };

            // Act
            var result = await _tripDetailsQuery.GetTripDetailsList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, b => b?.TripSearchRequest?.BookingReference == "7000858");
            var holidayProduct = result.FirstOrDefault(x => x.TripSearchRequest.BookingReference == "7000858");
            Assert.NotNull(holidayProduct);
            var airlineProduct = holidayProduct?.Product?.FirstOrDefault(x => x.Reference.PnrLocator == "DVSQLF");
            Assert.NotNull(airlineProduct.Passengers);
            Assert.NotEmpty(airlineProduct.Passengers);
            Assert.Contains(airlineProduct?.Passengers, v => v?.LeadPassenger == true);
        }
    }
}
