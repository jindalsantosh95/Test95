using HotChocolate;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsSlice")]
    public class Slice
    {
        public string? Id { get; set; }
        public int? DistanceMiles { get; set; }
        public int? Ordinal { get; set; }
        public DateTime? ScheduledDepartureLocalDateTime { get; set; }
        public DateTime? ScheduledDepartureUtcDateTime { get; set; }
        public DateTime? ScheduledArrivalLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalUtcDateTime { get; set; }
        public DateTime? ScheduledArrivalBasedOnDepartureLocalDateTime { get; set; }
        public DateTime? EstimatedDepartureLocalDateTime { get; set; }
        public DateTime? EstimatedDepartureUtcDateTime { get; set; }
        public DateTime? EstimatedArrivalLocalDateTime { get; set; }
        public DateTime? EstimatedArrivalUtcDateTime { get; set; }
        public DateTime? ActualDepartureLocalDateTime { get; set; }
        public DateTime? ActualDepartureUtcDateTime { get; set; }
        public DateTime? ActualArrivalLocalDateTime { get; set; }
        public DateTime? ActualArrivalUtcDateTime { get; set; }
        public AirportDetails? Origin { get; set; }
        public AirportDetails? Destination { get; set; }
        public IEnumerable<Segment>? Segments { get; set; }
        public string? CheckinStatus { get; set; }
        public string? Duration { get; set; }

        // Accept the Order Data contract without changing the public duration field.
        [Newtonsoft.Json.JsonProperty("totalSliceDuration")]
        private string? OrderDataDuration { set => Duration = value; }
        public bool CheckInEligible { get; set; }
        public DateTime? CheckInOpens { get; set; }
        public LocalDateTime? ScheduledDepartureLocal { get; set; }
        public LocalDateTime? ScheduledArrivalLocal { get; set; }
        public LocalDateTime? ScheduledArrivalBasedOnDepartureLocal { get; set; }
        public LocalDateTime? EstimatedDepartureLocal { get; set; }
        public LocalDateTime? EstimatedArrivalLocal { get; set; }
        public LocalDateTime? ActualDepartureLocal { get; set; }
        public LocalDateTime? ActualArrivalLocal { get; set; }
        public LocalDateTime? CheckInOpensLocal { get; set; }
        public DateTime? ScheduledDepartureUtc { get; set; }
        public DateTime? ScheduledArrivalUtc { get; set; }
        public DateTime? EstimatedDepartureUtc { get; set; }
        public DateTime? EstimatedArrivalUtc { get; set; }
        public DateTime? ActualDepartureUtc { get; set; }
        public DateTime? ActualArrivalUtc { get; set; }
        public bool? AreTravelDocsComplete { get; set; }
        public string TravelDocsStatusSummary { get; set; } = string.Empty;

        public static explicit operator Slice((Infrastructure.Models.TripMS.Slice slice, IEnumerable<CabinRefData> cabinDetails, IEnumerable<AirlineRefData> airlineDetails) source) =>
            (Slice)(source.slice, source.cabinDetails, source.airlineDetails, null);

        public static explicit operator Slice((Infrastructure.Models.TripMS.Slice slice, IEnumerable<CabinRefData> cabinDetails, IEnumerable<AirlineRefData> airlineDetails, Infrastructure.Models.CheckInMS.SliceRequirement travelRequirement) source)
            => (Slice)(source.slice, source.cabinDetails, source.airlineDetails, source.travelRequirement, null);

        public static explicit operator Slice((Infrastructure.Models.TripMS.Slice slice, IEnumerable<CabinRefData> cabinDetails, IEnumerable<AirlineRefData> airlineDetails, Infrastructure.Models.CheckInMS.SliceRequirement travelRequirement, string? pnrLocator) source)
        {
            var (slice, cabinDetails, airlineDetails, travelRequirement, pnrLocator) = source;

            if (slice == null) return null;

            var segment = slice.Segments.FirstOrDefault();
            var isCancelled = CheckInEligibilityHelper.IsCancelled(slice);
            var setcheckInOpens = !isCancelled && (segment?.ScheduledDepartureUtcDateTime > DateTime.UtcNow) && (segment?.ScheduledDepartureUtcDateTime.Date <= DateTime.UtcNow.Date.AddDays(7));

            bool? areTravelDocsComplete = travelRequirement?.SecondaryDocumentStatus switch
            {
                "COMPLETE" or "NOT_REQUIRED" => true,
                "PROVIDE_PASSPORT_FIRST" or "REQUIRED" => false,
                _ => null
            };

            return new Slice()
            {
                CheckinStatus = slice.CheckinStatus,
                Destination = (AirportDetails)slice.Destination,
                DistanceMiles = slice.DistanceMiles,
                Id = slice.Id,
                Ordinal = slice.Ordinal,
                Origin = (AirportDetails)slice.Origin,
                ScheduledArrivalBasedOnDepartureLocalDateTime = slice.ScheduledArrivalBasedOnDepartureLocalDateTime,
                ScheduledArrivalLocalDateTime = slice.ScheduledArrivalLocalDateTime,
                ScheduledArrivalUtcDateTime = slice.ScheduledArrivalUtcDateTime,
                ScheduledDepartureLocalDateTime = slice.ScheduledDepartureLocalDateTime,
                ScheduledDepartureUtcDateTime = slice.ScheduledDepartureUtcDateTime,
                EstimatedDepartureLocalDateTime = slice.EstimatedDepartureLocalDateTime,
                EstimatedDepartureUtcDateTime = slice.EstimatedDepartureUtcDateTime,
                EstimatedArrivalLocalDateTime = slice.EstimatedArrivalLocalDateTime,
                EstimatedArrivalUtcDateTime = slice.EstimatedArrivalUtcDateTime,
                ActualDepartureLocalDateTime = slice.ActualDepartureLocalDateTime,
                ActualDepartureUtcDateTime = slice.ActualDepartureUtcDateTime,
                ActualArrivalLocalDateTime = slice.ActualArrivalLocalDateTime,
                ActualArrivalUtcDateTime = slice.ActualArrivalUtcDateTime,
                ScheduledDepartureUtc = slice.ScheduledDepartureUtcDateTime,
                ScheduledArrivalUtc = slice.ScheduledArrivalUtcDateTime,
                EstimatedDepartureUtc = slice.EstimatedDepartureUtcDateTime,
                EstimatedArrivalUtc = slice.EstimatedArrivalUtcDateTime,
                ActualDepartureUtc = slice.ActualDepartureUtcDateTime,
                ActualArrivalUtc = slice.ActualArrivalUtcDateTime,
                AreTravelDocsComplete = areTravelDocsComplete,
                TravelDocsStatusSummary = areTravelDocsComplete switch
                {
                    true => "Checklist complete",
                    false => "Documents required",
                    null => string.Empty
                },
                Segments = slice.Segments?.Select(x => (Segment)(x, cabinDetails, airlineDetails, pnrLocator)),
                Duration = slice.TotalSliceDuration,
                CheckInEligible = isCancelled ? false : slice.CheckInEligible,
                CheckInOpens = setcheckInOpens ? segment?.ScheduledDepartureLocalDateTime.AddHours(-24) : null,
                ScheduledDepartureLocal = DateTimeHelper.ToLocal(slice.ScheduledDepartureLocalDateTime),
                ScheduledArrivalLocal = DateTimeHelper.ToLocal(slice.ScheduledArrivalLocalDateTime),
                ScheduledArrivalBasedOnDepartureLocal = DateTimeHelper.ToLocal(slice.ScheduledArrivalBasedOnDepartureLocalDateTime),
                EstimatedDepartureLocal = DateTimeHelper.ToLocal(slice.EstimatedDepartureLocalDateTime),
                EstimatedArrivalLocal = DateTimeHelper.ToLocal(slice.EstimatedArrivalLocalDateTime),
                ActualDepartureLocal = DateTimeHelper.ToLocal(slice.ActualDepartureLocalDateTime),
                ActualArrivalLocal = DateTimeHelper.ToLocal(slice.ActualArrivalLocalDateTime),
                CheckInOpensLocal = setcheckInOpens ? DateTimeHelper.ToLocal(segment?.ScheduledDepartureLocalDateTime.AddHours(-24)) : null,
            };
        }

        public static explicit operator Slice((Infrastructure.Models.HolidaysMS.TripsCore.Slice slice, IEnumerable<AirportRefData> refAirportData) source)
            => (Slice)(source.slice, source.refAirportData, Enumerable.Empty<AirlineRefData>());

        public static explicit operator Slice((Infrastructure.Models.HolidaysMS.TripsCore.Slice slice, IEnumerable<AirportRefData> refAirportData, IEnumerable<AirlineRefData> airlineRefData) source)
            => (Slice)(source.slice, source.refAirportData, source.airlineRefData, true);

        public static explicit operator Slice((Infrastructure.Models.HolidaysMS.TripsCore.Slice slice, IEnumerable<AirportRefData> refAirportData, IEnumerable<AirlineRefData> airlineRefData, bool isSupportedFlight) source)
        {
            var (slice, refAirportData, airlineRefData, isSupportedFlight) = source;

            if (slice == null) return null;

            var mappedSlice = new Slice()
            {
                CheckinStatus = slice.CheckinStatus,
                Destination = (AirportDetails)(slice.Destination, refAirportData),
                //DistanceMiles = source.DistanceMiles,
                Id = slice.Id,
                Ordinal = slice.Ordinal,
                Origin = (AirportDetails)(slice.Origin, refAirportData),
                Segments = slice.Segments?.Select(x => (Segment)(x, refAirportData, airlineRefData, isSupportedFlight)),
                Duration = slice.TotalSliceDuration,
                ScheduledDepartureLocal = DateTimeHelper.ToLocal(slice.ScheduledDepartureLocal),
                ScheduledArrivalLocal = DateTimeHelper.ToLocal(slice.ScheduledArrivalLocal)
            };

            mappedSlice.CheckInEligible = CheckInEligibilityHelper.IsCancelled(slice) ? false : slice.CheckInEligible;

            return mappedSlice;
        }
    }
}
