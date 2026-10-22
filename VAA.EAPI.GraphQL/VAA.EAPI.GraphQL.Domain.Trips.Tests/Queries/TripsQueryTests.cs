using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Interface.Features;
using VAA.EAPI.GraphQL.Domain.Trips.Queries;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Domain.Trips.Services.Features;
using VAA.EAPI.GraphQL.Domain.Trips.Tests.Helpers;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Queries
{
    public class TripsQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly ILogger<TripsQuery> _loggerTrip;
        private readonly HttpConnector _httpConnector;
        private readonly IReferenceServiceProvider _mockReferenceServiceProvider;
        private readonly ITripsDetailsQueryProvider _mockTripsDetailsQueryProvider;
        private readonly IPassengerDetailsProvider _mockPassengerDetailsServiceProvider;
        private readonly IBasketProvider _mockbasketProvider;
        private readonly ITripDetailsFeatureProvider _mockTripDetailsFeatureProvider;
        private readonly ITravelRequirementsQueryProvider _travelRequirementsQueryProvider;
        private readonly IOperationalScheduleProvider _mockOperationalScheduleProvider;
        private readonly IDateTimeProvider _mockDateTimeProvider;
        private readonly TripsQuery _tripsQuery;
       

        public TripsQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerTrip = Microsoft.Extensions.Logging.Abstractions.NullLogger<TripsQuery>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _mockReferenceServiceProvider = new ReferenceServiceProvider(_httpConnector);
            _mockTripsDetailsQueryProvider = new TripsDetailsQueryProvider(_httpConnector);
            _mockPassengerDetailsServiceProvider = new PassengerDetailsProvider(_httpConnector);
            _mockOperationalScheduleProvider = new OperationalScheduleProvider(_httpConnector);
            _mockDateTimeProvider = new TestDateTimeProvider(new DateTime(2025, 12, 23));
            _mockTripDetailsFeatureProvider = new TripDetailsFeatureProvider(_mockOperationalScheduleProvider, _mockDateTimeProvider);
            _mockbasketProvider = new BasketProvider(_httpConnector);
            _travelRequirementsQueryProvider = new TravelRequirementsQueryProvider(_httpConnector);
            _tripsQuery = new TripsQuery(_mockTripsDetailsQueryProvider, _mockReferenceServiceProvider, _mockPassengerDetailsServiceProvider, _mockbasketProvider, _loggerTrip, _mockTripDetailsFeatureProvider, _travelRequirementsQueryProvider);
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task TripQuery_VsEconomySegment_ReturnsPotentialUpgradeAvailable()
        {
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "IROP01",
                GivenNames = "TEST",
                Surname = "USER"
            };

            var booking = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);
            var segment = Assert.Single(booking.Product!.Single().Slices!.Single().Segments!);

            Assert.True(segment.PotentialUpgradeAvailable);
            Assert.True(booking.PotentialUpgradeAvailable);
        }

        [Fact]
        public async Task TripQuery_VsUpperClassSegment_ReturnsPotentialUpgradeUnavailable()
        {
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABCDEF",
                GivenNames = "givenName",
                Surname = "surName"
            };

            var booking = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);
            var segment = Assert.Single(booking.Product!.Single().Slices!.Single().Segments!);

            Assert.False(segment.PotentialUpgradeAvailable);
            Assert.False(booking.PotentialUpgradeAvailable);
        }

        [Fact]
        public async Task QueryTripsDetailsForTrip_ValidId_ReturnsResponse()
        {
            // Arrange
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABCDEF",
                GivenNames = "givenName",
                Surname = "surName"
            }; 
            

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Passengers?.Any());
            Assert.True(result.Product?.Any());
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Pos);
            Assert.True(result.Pos.CityCode == "LON");
            Assert.Equal("REVENUE", result.ReservationType);
            
            var slice = result.Product?.First().Slices?.First();
            var passengerSlice = result.Passengers?.First().Slices?.First();
            Assert.True(passengerSlice?.AreTravelDocsComplete);
            Assert.Equal("Checklist complete", passengerSlice?.TravelDocsStatusSummary);
            var segment = slice?.Segments?.First();
            var leg = segment?.Legs?.First();

            Assert.NotNull(segment);
            Assert.Equal(new DateTime(2025, 12, 6, 8, 55, 0), segment.PopsOpenDateTimeUtc);
            Assert.Equal(new DateTime(2025, 12, 12, 8, 55, 0), segment.PopsCloseDateTimeUtc);
            
            if (slice?.ScheduledDepartureLocalDateTime.HasValue == true)
                Assert.NotNull(slice.ScheduledDepartureLocal);
            if (slice?.ScheduledArrivalLocalDateTime.HasValue == true)
                Assert.NotNull(slice.ScheduledArrivalLocal);
            if (slice?.ScheduledDepartureUtcDateTime.HasValue == true)
                Assert.NotNull(slice.ScheduledDepartureUtc);
            if (slice?.ScheduledArrivalUtcDateTime.HasValue == true)
                Assert.NotNull(slice.ScheduledArrivalUtc);
            if (slice?.EstimatedDepartureUtcDateTime.HasValue == true)
                Assert.NotNull(slice.EstimatedDepartureUtc);
            if (slice?.EstimatedArrivalUtcDateTime.HasValue == true)
                Assert.NotNull(slice.EstimatedArrivalUtc);
            if (slice?.ActualDepartureUtcDateTime.HasValue == true)
                Assert.NotNull(slice.ActualDepartureUtc);
            if (slice?.ActualArrivalUtcDateTime.HasValue == true)
                Assert.NotNull(slice.ActualArrivalUtc);
            if (segment?.ScheduledDepartureLocalDateTime.HasValue == true)
                Assert.NotNull(segment.ScheduledDepartureLocal);
            if (segment?.ScheduledArrivalLocalDateTime.HasValue == true)
                Assert.NotNull(segment.ScheduledArrivalLocal);
            if (segment?.ScheduledDepartureUtcDateTime.HasValue == true)
                Assert.NotNull(segment.ScheduledDepartureUtc);
            if (segment?.ScheduledArrivalUtcDateTime.HasValue == true)
                Assert.NotNull(segment.ScheduledArrivalUtc);
            if (segment?.EstimatedDepartureUtcDateTime.HasValue == true)
                Assert.NotNull(segment.EstimatedDepartureUtc);
            if (segment?.EstimatedArrivalUtcDateTime.HasValue == true)
                Assert.NotNull(segment.EstimatedArrivalUtc);
            if (segment?.ActualDepartureUtcDateTime.HasValue == true)
                Assert.NotNull(segment.ActualDepartureUtc);
            if (segment?.ActualArrivalUtcDateTime.HasValue == true)
                Assert.NotNull(segment.ActualArrivalUtc);
            if (leg?.ScheduledDepartureLocalDateTime.HasValue == true)
                Assert.NotNull(leg.ScheduledDepartureLocal);
            if (leg?.ScheduledArrivalLocalDateTime.HasValue == true)
                Assert.NotNull(leg.ScheduledArrivalLocal);
            if (leg?.ScheduledDepartureUtcDateTime.HasValue == true)
                Assert.NotNull(leg.ScheduledDepartureUtc);
            if (leg?.ScheduledArrivalUtcDateTime.HasValue == true)
                Assert.NotNull(leg.ScheduledArrivalUtc);
            if (leg?.EstimatedDepartureUtcDateTime.HasValue == true)
                Assert.NotNull(leg.EstimatedDepartureUtc);
            if (leg?.EstimatedArrivalUtcDateTime.HasValue == true)
                Assert.NotNull(leg.EstimatedArrivalUtc);
            if (leg?.ActualDepartureUtcDateTime.HasValue == true)
                Assert.NotNull(leg.ActualDepartureUtc);
            if (leg?.ActualArrivalUtcDateTime.HasValue == true)
                Assert.NotNull(leg.ActualArrivalUtc);
        }

        [Fact]
        public async Task TripQuery_NonVSOperatingCarrier_ReturnsNullPopsTimings()
        {
            // Arrange
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABC002",
                GivenNames = "givenName",
                Surname = "surName"
            };

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            var segment = result.Product?.First().Slices?.First().Segments?.First();
            Assert.NotNull(segment);
            Assert.Null(segment.PopsOpenDateTimeUtc);
            Assert.Null(segment.PopsCloseDateTimeUtc);
        }

        [Fact]
        public async Task TripQuery_VSOperatingCarrier_WithoutScheduledDeparture_ReturnsNullPopsTimings()
        {
            // Arrange
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABC003",
                GivenNames = "givenName",
                Surname = "surName"
            };

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            var segment = result.Product?.First().Slices?.First().Segments?.First();
            Assert.NotNull(segment);
            Assert.Equal("VS", segment.OperatingCarrier?.CarrierCode);
            Assert.Null(segment.PopsOpenDateTimeUtc);
            Assert.Null(segment.PopsCloseDateTimeUtc);
        }

        [Fact]
        public async Task TripQuery_ValidPNRWithAircraftDetails_ReturnsNullOutside3Days()
        {
            // Arrange
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABCXYZ",
                GivenNames = "givenName",
                Surname = "surName"
            };

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Passengers?.Any());
            Assert.True(result.Product?.Any());
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Pos);
            Assert.True(result.Pos.CityCode == "LON");
            Assert.Null(result.Product?.First().Slices?.First().Segments?.First().Legs?.First().Aircraft?.TailNumber);
            Assert.Null(result.Product?.First().Slices?.First().Segments?.First().Legs?.First().Aircraft?.PlaneName);
        }

        [Fact]
        public async Task TripQuery_ValidPNRNoAircraftDetails_ReturnsSuccessInside3Days()
        {
            // Arrange           
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABC123",
                GivenNames = "givenName",
                Surname = "surName"
            };

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Passengers?.Any());
            Assert.True(result.Product?.Any());
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Pos);
            Assert.True(result.Pos.CityCode == "LON");
            Assert.Null(result.Product?.First().Slices?.First().Segments?.First().Legs?.First().Aircraft?.TailNumber);
            Assert.Null(result.Product?.First().Slices?.First().Segments?.First().Legs?.First().Aircraft?.PlaneName);
        }

        [Fact]
        public async Task TripQuery_ValidPNRWithAircraftDetails_ReturnsSuccessInside3Days()
        {
            // Arrange
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABC000",
                GivenNames = "givenName",
                Surname = "surName"
            };

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Passengers?.Any());
            Assert.True(result.Product?.Any());
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Pos);
            Assert.True(result.Pos.CityCode == "LON");
            Assert.NotNull(result.Product?.First().Slices?.First().Segments?.First().Legs?.First().Aircraft?.TailNumber);
            Assert.NotNull(result.Product?.First().Slices?.First().Segments?.First().Legs?.First().Aircraft?.PlaneName);
        }

        [Fact]
        public async Task TripQuery_ValidPNRWithAirlneDetailsFromReference_ReturnsSuccess()
        {
            // Arrange
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABC001",
                GivenNames = "givenName",
                Surname = "surName"
            };

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);           
            Assert.Equal("Test Airline", result.Product?.First().Slices?.First().Segments?.FirstOrDefault()?.OperatingCarrier?.CarrierName);
            Assert.Equal("Test Airline", result.Product?.First().Slices?.First().Segments?.FirstOrDefault()?.MarketingCarrier?.CarrierName);
            Assert.Equal("Test Airline", result.Product?.First().Slices?.First().Segments?.FirstOrDefault()?.Legs?.FirstOrDefault()?.Operating?.CarrierName);
        }

        [Fact]
        public async Task TripQuery_ValidPNRWithAirlneDetailsNotInReference_ReturnsSuccess()
        {
            // Arrange
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABC002",
                GivenNames = "givenName",
                Surname = "surName"
            };

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("It is a Long Test Airline Delta Value", result.Product?.First().Slices?.First().Segments?.FirstOrDefault()?.OperatingCarrier?.CarrierName);
            Assert.Equal("Virgin Atlantic For Test", result.Product?.First().Slices?.First().Segments?.FirstOrDefault()?.MarketingCarrier?.CarrierName);
            Assert.Equal("It is a Long Test Airline Delta Value", result.Product?.First().Slices?.First().Segments?.FirstOrDefault()?.Legs?.FirstOrDefault()?.Operating?.CarrierName);

        }

        [Fact]
        public async Task TripQuery_ValidateAirlineTitleAndText_ForKoreanAirlines()
        {
            // Arrange
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABCKE1",
                GivenNames = "givenName",
                Surname = "surName"
            };

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("This flight is with Korean Air", result.Product?.First().Slices?.First().Segments?.FirstOrDefault()?.OtherAirlineOperatedHelpTitle);
            Assert.Equal(ConfigReader.Get(Constants.Environment.KoreanAirlineOperatedHelpText), result.Product?.First().Slices?.First().Segments?.FirstOrDefault()?.OtherAirlineOperatedHelpText);            
        }

        [Fact]
        public async Task TripQuery_ValidateAirlineTitleAndText_ForVSAirlines()
        {
            // Arrange
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABC321",
                GivenNames = "givenName",
                Surname = "surName"
            };
          
            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Product?.First().Slices?.First().Segments?.FirstOrDefault()?.OtherAirlineOperatedHelpTitle);
            Assert.Null(result.Product?.First().Slices?.First().Segments?.FirstOrDefault()?.OtherAirlineOperatedHelpText);
        }

        [Fact]
        public async Task TripQuery_ValidateAirlineTitleAndText_ForOtherAirlines()
        {
            // Arrange
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABC002",
                GivenNames = "givenName",
                Surname = "surName"
            };

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("This flight is with It is a Long Test Airline Delta Value", result.Product?.First().Slices?.First().Segments?.FirstOrDefault()?.OtherAirlineOperatedHelpTitle);
            Assert.Equal(ConfigReader.Get(Constants.Environment.OtherAirlineOperatedHelpText), result.Product?.First().Slices?.First().Segments?.FirstOrDefault()?.OtherAirlineOperatedHelpText);
        }

        [Fact]
        public async Task TripQuery_ValidPNRWithCancelledSegment_ReturnsNullCheckInOpensWithin7Days()
        {
            // Arrange
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABC321",
                GivenNames = "givenName",
                Surname = "surName"
            };

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Passengers?.Any());
            Assert.True(result.Product?.Any());           
            Assert.Null(result.Product?.First().Slices?.First().CheckInOpens);
        }

        [Fact]
        public async Task TripQuery_IropsCancelDirty_ReturnsFalseCheckInEligibility()
        {
            // Arrange
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "IROP01",
                GivenNames = "TEST",
                Surname = "USER"
            };

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Product?.First().CheckInEligible);
            Assert.False(result.Product?.First().Slices?.First().CheckInEligible);
            Assert.Null(result.Product?.First().Slices?.First().CheckInOpens);
        }

        [Fact]
        public async Task TripQuery_ValidPNRWithSegmentWith7Days_ReturnsCheckInOpensValue()
        {
            // Arrange           
            var tripDetailsRequest = new Models.Trips.Virgin.TripsDetailsRequest
            {
                ConfirmationNum = "ABC123",
                GivenNames = "givenName",
                Surname = "surName"
            };

            // Act
            var result = await _tripsQuery.GetTripsDetails(tripDetailsRequest, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Passengers?.Any());
            Assert.True(result.Product?.Any());
            Assert.Multiple(() =>
            {
                var nextSegmentDateTime = result.Product?.First().Slices?.First().Segments?.First().ScheduledDepartureUtc;
                if (nextSegmentDateTime != null && nextSegmentDateTime.Value.Date > DateTime.UtcNow.Date && nextSegmentDateTime.Value.Date.AddDays(-7) < DateTime.UtcNow.Date)
                {
                    Assert.NotNull(result.Product?.First().Slices?.First().CheckInOpensLocal);
                    Assert.Equal(result.Product?.First().Slices?.First().Segments?.First()?.ScheduledDepartureUtc.Value.AddHours(-24), result.Product?.First().Slices?.First().CheckInOpens);
                }
                else
                {
                    Assert.Null(result.Product?.First().Slices?.First().CheckInOpensLocal);
                }

            });
          
        }

        [Fact]
        public void RegularFlight_IropsCancellation_ReturnsIneligibleAtProductAndSliceLevel()
        {
            var sourceSlice = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Slice
            {
                CheckInEligible = true,
                Segments = new[]
                {
                    new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Segment
                    {
                        OperationalStatus = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.OperationalStatus
                        {
                            Type = "IROPS",
                            SubType = "CANCELLED"
                        },
                        Legs = new[] { new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Leg() }
                    }
                }
            };
            var sourceProduct = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Product
            {
                CheckInEligible = true,
                Slices = new[] { sourceSlice }
            };

            var product = (Models.Trips.Virgin.Product)(sourceProduct, Array.Empty<Models.ReferenceData.CabinRefData>(), Array.Empty<Models.ReferenceData.AirlineRefData>());

            Assert.False(product.CheckInEligible);
            Assert.False(product.Slices.Single().CheckInEligible);
        }

        [Theory]
        [InlineData("IROP Cancel Dirty No Protect", true)]
        [InlineData("IROP Cancel Cleaned", false)]
        public void RegularFlight_FutureIropsSlice_OnlyActiveCancellationSuppressesCheckInOpens(string subType, bool isCancelled)
        {
            var departure = DateTime.UtcNow.AddDays(2);
            var sourceSlice = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Slice
            {
                CheckInEligible = true,
                Segments = new[]
                {
                    new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Segment
                    {
                        ScheduledDepartureUtcDateTime = departure,
                        ScheduledDepartureLocalDateTime = departure,
                        OperationalStatus = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.OperationalStatus
                        {
                            Type = "IROP",
                            SubType = subType
                        },
                        Legs = new[] { new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Leg { Status = "ON_TIME" } }
                    }
                }
            };

            var slice = (Models.Trips.Virgin.Slice)(sourceSlice, Array.Empty<Models.ReferenceData.CabinRefData>(), Array.Empty<Models.ReferenceData.AirlineRefData>());

            Assert.Equal(!isCancelled, slice.CheckInEligible);
            Assert.Equal(isCancelled ? (DateTime?)null : departure.AddHours(-24), slice.CheckInOpens);
        }

        [Fact]
        public void RegularFlight_ProtectedCancelledSlice_DoesNotDisableProductEligibility()
        {
            var protectedSlice = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Slice
            {
                CheckInEligible = true,
                IropProtect = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.IropProtect
                {
                    Protect = true
                },
                Segments = new[]
                {
                    new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Segment
                    {
                        OperationalStatus = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.OperationalStatus
                        {
                            Type = "IROP",
                            SubType = "IROP Cancel Dirty No Protect"
                        },
                        Legs = new[] { new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Leg() }
                    }
                }
            };

            var product = (Models.Trips.Virgin.Product)(new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Product
            {
                CheckInEligible = true,
                Slices = new[] { protectedSlice }
            }, Array.Empty<Models.ReferenceData.CabinRefData>(), Array.Empty<Models.ReferenceData.AirlineRefData>());

            Assert.True(product.CheckInEligible);
            Assert.Empty(product.Slices);
        }

        [Fact]
        public void RegularFlight_NonCancelledLongLayover_PreservesEligibility()
        {
            var firstDeparture = DateTime.UtcNow.AddHours(12);
            var sourceSlice = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Slice
            {
                CheckInEligible = true,
                Segments = new[]
                {
                    new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Segment
                    {
                        ScheduledDepartureUtcDateTime = firstDeparture,
                        ScheduledDepartureLocalDateTime = firstDeparture,
                        Legs = new[] { new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Leg { Status = "CONFIRMED" } }
                    },
                    new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Segment
                    {
                        ScheduledDepartureUtcDateTime = firstDeparture.AddHours(10),
                        ScheduledDepartureLocalDateTime = firstDeparture.AddHours(10),
                        Legs = new[] { new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Leg { Status = "CONFIRMED" } }
                    }
                }
            };

            var slice = (Models.Trips.Virgin.Slice)(sourceSlice, Array.Empty<Models.ReferenceData.CabinRefData>(), Array.Empty<Models.ReferenceData.AirlineRefData>());
            var product = (Models.Trips.Virgin.Product)(new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.Product
            {
                CheckInEligible = true,
                Slices = new[] { sourceSlice }
            }, Array.Empty<Models.ReferenceData.CabinRefData>(), Array.Empty<Models.ReferenceData.AirlineRefData>());

            Assert.True(slice.CheckInEligible);
            Assert.NotNull(slice.CheckInOpens);
            Assert.True(product.CheckInEligible);
        }

        [Fact]
        public void HolidayFlight_CancelledLeg_ReturnsIneligible()
        {
            var sourceSlice = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Slice
            {
                CheckInEligible = true,
                Segments = new[]
                {
                    new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Segment
                    {
                        Legs = new[] { new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Leg { Status = "CANCELLED" } }
                    }
                }
            };

            var slice = (Models.Trips.Virgin.Slice)(sourceSlice, Array.Empty<Models.ReferenceData.AirportRefData>(), Array.Empty<Models.ReferenceData.AirlineRefData>());

            Assert.False(slice.CheckInEligible);
        }

        [Fact]
        public void HolidayFlight_IropsCancellation_ReturnsIneligible()
        {
            var sourceSlice = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Slice
            {
                CheckInEligible = true,
                Segments = new[]
                {
                    new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Segment
                    {
                        OperationalStatus = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.OperationalStatus
                        {
                            Type = "IROP",
                            SubType = "IROP Cancel Dirty No Protect"
                        },
                        Legs = new[] { new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Leg { Status = "ON_TIME" } }
                    }
                }
            };

            var slice = (Models.Trips.Virgin.Slice)(sourceSlice, Array.Empty<Models.ReferenceData.AirportRefData>(), Array.Empty<Models.ReferenceData.AirlineRefData>());

            Assert.False(slice.CheckInEligible);
        }

        [Fact]
        public void HolidayFlight_NonCancelledSlice_PreservesEligibility()
        {
            var sourceSlice = new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Slice
            {
                CheckInEligible = true,
                Segments = new[]
                {
                    new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Segment
                    {
                        Legs = new[] { new global::VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore.Leg { Status = "ON_TIME" } }
                    }
                }
            };

            var slice = (Models.Trips.Virgin.Slice)(sourceSlice, Array.Empty<Models.ReferenceData.AirportRefData>(), Array.Empty<Models.ReferenceData.AirlineRefData>());

            Assert.True(slice.CheckInEligible);
        }
    }
}
