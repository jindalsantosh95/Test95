#nullable enable
using System;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using Scv = VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;
using Trips = VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore
{
    public static class ScvBookingResponseMapper
    {
        public static Trips.TripDetailsResponse Map(Scv.OrderDoc source)
        {
            ArgumentNullException.ThrowIfNull(source);
            var reference = source.OrderDetails?.BookingReferences?.FirstOrDefault();
            var creationDate = ParseDate(source.OrderDetails?.CreationDateTime?.UTCDateTime, utc: true);
            if (creationDate is null && source.OrderID?.Length > 7 &&
                DateTime.TryParseExact(source.OrderID[^7..], "ddMMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var bookingDate))
                creationDate = bookingDate;
            var passengers = source.CustomerDetails?.Select(customer => (Trips.Passenger)customer).ToArray() ?? [];
            // Allowlist the existing trip contract; never serialize the storage graph.
            return new Trips.TripDetailsResponse
            {
                Id = source.OrderID,
                ReservationType = source.CustomerDetails?.Any(customer => customer.Bookingtype?.Trim().Is("group") == true) == true
                    ? "group"
                    : source.CustomerDetails?.FirstOrDefault()?.Bookingtype?.Trim(),
                Pos = new Trips.Pos { CityCode = source.OriginalPOS?.CityCode, CountryCode = source.OriginalPOS?.CountryCode },
                Passengers = passengers,
                Product = [new Trips.Product
                {
                    BookingType = "AIRLINE",
                    Reference = new Trips.Reference
                    {
                        AirlineDesignator = source.OrderDetails?.OwnerAirline,
                        PnrLocator = reference?.RLOC ?? (source.OrderID?.Length > 7 ? source.OrderID[..^7] : null),
                        CreatedDate = creationDate?.ToString("O", CultureInfo.InvariantCulture),
                        BookingDate = creationDate
                    },
                    Passengers = passengers,
                    CheckInEligible = false,
                    // SCV does not provide the live Trips journey grouping: retain each segment in order.
                    Slices = source.SegmentDetails?.Select((segment, index) => MapSlice(segment, index + 1)).ToArray() ?? []
                }]
            };
        }

        private static Trips.Slice MapSlice(Scv.SegmentDetail source, int ordinal)
        {
            var segment = new Trips.Segment
            {
                Id = source.SegmentNumber,
                Ordinal = ordinal,
                FlightSegmentId = source.ProductId,
                Origin = new Trips.AirportDetails { Code = source.OriginAirportCode },
                Destination = new Trips.AirportDetails { Code = source.DestinationAirportCode },
                MarketingCarrier = new Trips.CarrierDetails { CarrierCode = source.MarketingCarrier, FlightNumber = source.MarketingFlightNumber },
                OperatingCarrier = new Trips.CarrierDetails { CarrierCode = source.OperatingCarrier, FlightNumber = source.OperatingFlightNumber },
                ScheduledDepartureLocalDateTime = ParseDate(source.ScheduledDepartureTime?.LocalDateTime),
                ScheduledDepartureUtcDateTime = ParseDate(source.ScheduledDepartureTime?.UTCDateTime, utc: true),
                ScheduledArrivalLocalDateTime = ParseDate(source.ScheduledArrivalTime?.LocalDateTime),
                ScheduledArrivalUtcDateTime = ParseDate(source.ScheduledArrivalTime?.UTCDateTime, utc: true),
                ActionCode = source.CurrentActionCode,
                BookedCabinClass = new Trips.CabinClass { Code = source.ClassOfService },
                TotalSegmentDuration = source.FlightDuration,
                Legs = source.Legs?.Select(MapLeg).ToArray() ?? []
            };
            return new Trips.Slice
            {
                Id = source.SegmentNumber,
                Ordinal = ordinal,
                Origin = segment.Origin,
                Destination = segment.Destination,
                ScheduledDepartureLocalDateTime = segment.ScheduledDepartureLocalDateTime,
                ScheduledDepartureUtcDateTime = segment.ScheduledDepartureUtcDateTime,
                ScheduledArrivalLocalDateTime = segment.ScheduledArrivalLocalDateTime,
                ScheduledArrivalUtcDateTime = segment.ScheduledArrivalUtcDateTime,
                TotalSliceDuration = source.FlightDuration,
                CheckInEligible = false,
                Segments = [segment]
            };
        }

        private static Trips.Leg MapLeg(Scv.Leg source) => new()
        {
            Id = source.LegSequenceNumber,
            Origin = new Trips.AirportDetails { Code = source.OriginAirportCode },
            Destination = new Trips.AirportDetails { Code = source.DestinationAirportCode },
            Operating = new Trips.CarrierDetails { CarrierCode = source.OperatingCarrier },
            ScheduledDepartureLocalDateTime = ParseDate(source.ScheduledDepartureTime?.LocalDateTime),
            ScheduledDepartureUtcDateTime = ParseDate(source.ScheduledDepartureTime?.UTCDateTime, utc: true),
            ScheduledArrivalLocalDateTime = ParseDate(source.ScheduledArrivalTime?.LocalDateTime),
            ScheduledArrivalUtcDateTime = ParseDate(source.ScheduledArrivalTime?.UTCDateTime, utc: true),
            OnAirDuration = source.LegDuration
        };

        private static DateTime? ParseDate(string? value, bool utc = false)
        {
            if (value.IsNullOrWhiteSpace()) return null;
            var styles = utc ? DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal : DateTimeStyles.None;
            return DateTime.TryParseExact(value, ["yyyy-MM-dd'T'HH:mm:ssK", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", "yyyy-MM-dd'T'HH:mmK"],
                CultureInfo.InvariantCulture, styles, out var date) ? date : null;
        }
    }
}
