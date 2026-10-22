using NodaTime;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;
using VAA.EAPI.GraphQL.Domain.Trips.Models.SSR;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Helpers
{
    public class LocalDateTimeConversionTests
    {
        [Fact]
        public void DateTimeHelper_ToLocal_ConvertsCorrectly()
        {
            // Arrange
            var dateTime = new DateTime(2024, 1, 15, 14, 30, 0);

            // Act
            var result = DateTimeHelper.ToLocal(dateTime);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2024, result.Value.Year);
            Assert.Equal(1, result.Value.Month);
            Assert.Equal(15, result.Value.Day);
            Assert.Equal(14, result.Value.Hour);
            Assert.Equal(30, result.Value.Minute);
        }

        [Fact]
        public void DateTimeHelper_ToLocal_HandlesNull()
        {
            // Act
            var result = DateTimeHelper.ToLocal(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void TripsSlice_LocalDateTimeProperties_ArePopulated()
        {
            // Arrange
            var msSlice = new Infrastructure.Models.TripMS.Slice
            {
                Id = "test-slice",
                ScheduledDepartureLocalDateTime = new DateTime(2024, 1, 15, 10, 0, 0),
                ScheduledArrivalLocalDateTime = new DateTime(2024, 1, 15, 12, 0, 0),
                EstimatedDepartureLocalDateTime = new DateTime(2024, 1, 15, 10, 15, 0),
                EstimatedArrivalLocalDateTime = new DateTime(2024, 1, 15, 12, 15, 0),
                Segments = new List<Infrastructure.Models.TripMS.Segment>()
            };

            // Act
            var slice = (Models.Trips.Virgin.Slice)(msSlice, Enumerable.Empty<Models.ReferenceData.CabinRefData>(), Enumerable.Empty<Models.ReferenceData.AirlineRefData>());

            // Assert
            Assert.NotNull(slice.ScheduledDepartureLocal);
            Assert.NotNull(slice.ScheduledArrivalLocal);
            Assert.NotNull(slice.EstimatedDepartureLocal);
            Assert.NotNull(slice.EstimatedArrivalLocal);
            Assert.Equal(10, slice.ScheduledDepartureLocal.Value.Hour);
            Assert.Equal(12, slice.ScheduledArrivalLocal.Value.Hour);
        }

        [Fact]
        public void HolidaysMsSlice_DurationIsMappedToGraphqlSliceDuration()
        {
            var msSlice = new Infrastructure.Models.HolidaysMS.TripsCore.Slice
            {
                Id = "holiday-slice",
                TotalSliceDuration = "PT18H15M"
            };

            var slice = (Models.Trips.Virgin.Slice)(msSlice,
                Enumerable.Empty<Models.ReferenceData.AirportRefData>(),
                Enumerable.Empty<Models.ReferenceData.AirlineRefData>());

            Assert.Equal("PT18H15M", slice.Duration);
        }

        [Fact]
        public void SSRFlightLeg_LocalDateTimeProperties_ArePopulated()
        {
            // Arrange
            var msFlightLeg = new Infrastructure.Models.SSRMS.FlightLeg
            {
                LegId = 1,
                ScheduledDepartureLocalDateTime = new DateTime(2024, 1, 15, 8, 0, 0),
                ScheduledArrivalLocalDateTime = new DateTime(2024, 1, 15, 10, 0, 0),
                OperatingAirlines = new List<Infrastructure.Models.SSRMS.Airline>()
            };

            // Act
            var flightLeg = (FlightLeg)msFlightLeg;

            // Assert
            Assert.NotNull(flightLeg.ScheduledDepartureLocal);
            Assert.NotNull(flightLeg.ScheduledArrivalLocal);
            Assert.Equal(8, flightLeg.ScheduledDepartureLocal.Value.Hour);
            Assert.Equal(10, flightLeg.ScheduledArrivalLocal.Value.Hour);
        }

        [Fact]
        public void UpsellSlice_LocalDateTimeProperties_ArePopulated()
        {
            // Arrange
            var msUpsellSlice = new Infrastructure.Models.UpsellOffer.Slice
            {
                Id = "upsell-slice",
                ScheduledDepartureLocalDateTime = new DateTime(2024, 1, 15, 16, 0, 0),
                ScheduledArrivalLocalDateTime = new DateTime(2024, 1, 15, 18, 0, 0)
            };

            // Act
            var upsellSlice = (Models.Upsell.Slice)(msUpsellSlice, Enumerable.Empty<Models.ReferenceData.AirportRefData>());

            // Assert
            Assert.NotNull(upsellSlice.ScheduledDepartureLocal);
            Assert.NotNull(upsellSlice.ScheduledArrivalLocal);
            Assert.Equal(16, upsellSlice.ScheduledDepartureLocal.Value.Hour);
            Assert.Equal(18, upsellSlice.ScheduledArrivalLocal.Value.Hour);
        }

        [Fact]
        public void UpsellLeg_LocalDateTimeProperties_ArePopulated()
        {
            // Arrange
            var msUpsellLeg = new Infrastructure.Models.UpsellOffer.Leg
            {
                Id = "upsell-leg",
                ScheduledDepartureLocalDateTime = new DateTime(2024, 1, 15, 20, 0, 0),
                ScheduledArrivalLocalDateTime = new DateTime(2024, 1, 15, 22, 0, 0),
                ScheduledArrivalBasedOnDepartureLocalDateTime = new DateTime(2024, 1, 15, 22, 30, 0)
            };

            // Act
            var upsellLeg = (Models.Upsell.Leg)(msUpsellLeg, Enumerable.Empty<Models.ReferenceData.AirportRefData>(), Enumerable.Empty<Models.ReferenceData.AirlineRefData>());

            // Assert
            Assert.NotNull(upsellLeg.ScheduledDepartureLocal);
            Assert.NotNull(upsellLeg.ScheduledArrivalLocal);
            Assert.NotNull(upsellLeg.ScheduledArrivalBasedOnDepartureLocal);
            Assert.Equal(20, upsellLeg.ScheduledDepartureLocal.Value.Hour);
            Assert.Equal(22, upsellLeg.ScheduledArrivalLocal.Value.Hour);
            Assert.Equal(22, upsellLeg.ScheduledArrivalBasedOnDepartureLocal.Value.Hour);
            Assert.Equal(30, upsellLeg.ScheduledArrivalBasedOnDepartureLocal.Value.Minute);
        }
    }
}
