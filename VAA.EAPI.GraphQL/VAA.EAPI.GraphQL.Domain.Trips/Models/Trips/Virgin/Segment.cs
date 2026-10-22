using HotChocolate;
using NodaTime;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using DateTimeHelper = VAA.EAPI.GraphQL.Domain.Trips.Helper.DateTimeHelper;
using TripsHelper = VAA.EAPI.GraphQL.Domain.Trips.Helper;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsSegment")]
    public class Segment
    {
        public string? Id { get; set; }
        public string? PnrLocator { get; set; }
        public int? Ordinal { get; set; }
        public CarrierDetails? OperatingCarrier { get; set; }
        public CarrierDetails? MarketingCarrier { get; set; }
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
        public DateTime? ScheduledDepartureUtc { get; set; }
        public DateTime? ScheduledArrivalUtc { get; set; }
        public DateTime? EstimatedDepartureUtc { get; set; }
        public DateTime? EstimatedArrivalUtc { get; set; }
        public DateTime? ActualDepartureUtc { get; set; }
        public DateTime? ActualArrivalUtc { get; set; }
        public DateTime? PopsOpenDateTimeUtc { get; set; }
        public DateTime? PopsCloseDateTimeUtc { get; set; }
        public LocalDateTime? ScheduledDepartureLocal { get; set; }
        public LocalDateTime? ScheduledArrivalLocal { get; set; }
        public LocalDateTime? ScheduledArrivalBasedOnDepartureLocal { get; set; }
        public LocalDateTime? EstimatedDepartureLocal { get; set; }
        public LocalDateTime? EstimatedArrivalLocal { get; set; }
        public LocalDateTime? ActualDepartureLocal { get; set; }
        public LocalDateTime? ActualArrivalLocal { get; set; }
        public string? LayoverDuration { get; set; }
        public AirportDetails? Origin { get; set; }
        public AirportDetails? Destination { get; set; }
        public string? ActionCode { get; set; }
        public IEnumerable<Leg>? Legs { get; set; }
        public string? FlightSegmentId { get; set; }
        public AirlineDetails? Operating { get; set; }
        public AirlineDetails? Marketing { get; set; }
        public CabinClass? BookedCabinClass { get; set; }
        public string? CheckinStatus { get; set; }
        public string? Duration { get; set; }

        // Accept the Order Data contract without changing the public duration field.
        [Newtonsoft.Json.JsonProperty("totalSegmentDuration")]
        private string? OrderDataDuration { set => Duration = value; }
        public string? FareBasisCode { get; set; }
        public bool SeatEligible { get; set; }
        public OperationalStatus? OperationalStatus { get; set; }
        public bool CheckedIn { get; set; }
        public bool? IsLayoverDurationValid { get; set; }
        public string? OtherAirlineOperatedHelpTitle { get; set; }
        public string? OtherAirlineOperatedHelpText { get; set; }
        public string? ManageFlightLink { get; set; }
        public bool PotentialUpgradeAvailable =>
            OperatingCarrier?.CarrierCode.Is(Constants.Application.VirginAtlanticCarrierCode) == true
            && Legs?.Any() == true
            && Legs.All(leg =>
                leg.CabinClass?.Code.Is("Y") == true
                || leg.CabinClass?.Code.Is("W") == true);

        public static explicit operator Segment((Infrastructure.Models.TripMS.Segment segment, IEnumerable<CabinRefData> cabinDetails, IEnumerable<AirlineRefData> airlineDetails) source)
            => (Segment)(source.segment, source.cabinDetails, source.airlineDetails, null);

        public static explicit operator Segment((Infrastructure.Models.TripMS.Segment segment, IEnumerable<CabinRefData> cabinDetails, IEnumerable<AirlineRefData> airlineDetails, string? pnrLocator) source)
        {
            var (segment, cabinDetails, airlineDetails, pnrLocator) = source;

            if (segment == null) return null;

            DateTime? scheduledDepartureUtc = segment.ScheduledDepartureUtcDateTime == default
                ? null
                : segment.ScheduledDepartureUtcDateTime;
            var (popsOpenDateTimeUtc, popsCloseDateTimeUtc) = GetPopsTimings(segment.OperatingCarrier?.CarrierCode, scheduledDepartureUtc);
            var (otherAirlineOperatedHelpTitle, otherAirlineOperatedHelpText) = GetOperatingCarrierHelpInfo(segment.OperatingCarrier?.CarrierCode, segment.OperatingCarrier?.CarrierName, airlineDetails);

            return new Segment
            {
                LayoverDuration = segment.Legs?.LastOrDefault()?.OnGroundDuration,
                ActionCode = segment.ActionCode,
                BookedCabinClass = (CabinClass)(segment.BookedCabinClass, Enumerable.Empty<CabinRefData>()),
                CheckinStatus = segment.CheckinStatus,
                Destination = (AirportDetails)segment.Destination,
                FlightSegmentId = segment.FlightSegmentId,
                Id = segment.Id,
                PnrLocator = pnrLocator,
                Legs = segment.Legs?.Select(x => (Leg)(x, cabinDetails, airlineDetails)),
                MarketingCarrier = (CarrierDetails)(segment.MarketingCarrier, airlineDetails),
                Marketing = null,
                OperatingCarrier = (CarrierDetails)(segment.OperatingCarrier, airlineDetails),
                Operating = null,
                Ordinal = segment.Ordinal,
                Origin = (AirportDetails)segment.Origin,
                ScheduledArrivalBasedOnDepartureLocalDateTime = segment.ScheduledArrivalBasedOnDepartureLocalDateTime,
                ScheduledArrivalLocalDateTime = segment.ScheduledArrivalLocalDateTime,
                ScheduledArrivalUtcDateTime = segment.ScheduledArrivalUtcDateTime,
                ScheduledDepartureLocalDateTime = segment.ScheduledDepartureLocalDateTime,
                ScheduledDepartureUtcDateTime = segment.ScheduledDepartureUtcDateTime,
                EstimatedDepartureLocalDateTime = segment.EstimatedDepartureLocalDateTime,
                EstimatedDepartureUtcDateTime = segment.EstimatedDepartureUtcDateTime,
                EstimatedArrivalLocalDateTime = segment.EstimatedArrivalLocalDateTime,
                EstimatedArrivalUtcDateTime = segment.EstimatedArrivalUtcDateTime,
                ActualDepartureLocalDateTime = segment.ActualDepartureLocalDateTime,
                ActualDepartureUtcDateTime = segment.ActualDepartureUtcDateTime,
                ActualArrivalLocalDateTime = segment.ActualArrivalLocalDateTime,
                ActualArrivalUtcDateTime = segment.ActualArrivalUtcDateTime,
                ScheduledDepartureUtc = segment.ScheduledDepartureUtcDateTime,
                ScheduledArrivalUtc = segment.ScheduledArrivalUtcDateTime,
                EstimatedDepartureUtc = segment.EstimatedDepartureUtcDateTime,
                EstimatedArrivalUtc = segment.EstimatedArrivalUtcDateTime,
                ActualDepartureUtc = segment.ActualDepartureUtcDateTime,
                ActualArrivalUtc = segment.ActualArrivalUtcDateTime,
                PopsOpenDateTimeUtc = popsOpenDateTimeUtc,
                PopsCloseDateTimeUtc = popsCloseDateTimeUtc,
                ScheduledDepartureLocal = TripsHelper.DateTimeHelper.ToLocal(segment.ScheduledDepartureLocalDateTime),
                ScheduledArrivalLocal = TripsHelper.DateTimeHelper.ToLocal(segment.ScheduledArrivalLocalDateTime),
                ScheduledArrivalBasedOnDepartureLocal = TripsHelper.DateTimeHelper.ToLocal(segment.ScheduledArrivalBasedOnDepartureLocalDateTime),
                EstimatedDepartureLocal = TripsHelper.DateTimeHelper.ToLocal(segment.EstimatedDepartureLocalDateTime),
                EstimatedArrivalLocal = TripsHelper.DateTimeHelper.ToLocal(segment.EstimatedArrivalLocalDateTime),
                ActualDepartureLocal = TripsHelper.DateTimeHelper.ToLocal(segment.ActualDepartureLocalDateTime),
                ActualArrivalLocal = TripsHelper.DateTimeHelper.ToLocal(segment.ActualArrivalLocalDateTime),
                Duration = segment.TotalSegmentDuration,
                FareBasisCode = segment.FareBasisCode,
                IsLayoverDurationValid = segment.Legs?.LastOrDefault()?.OnGroundDuration is null ? null : segment.Legs.Last().OnGroundDuration.StartsWith("-") != true,
                SeatEligible = segment.Legs.First().SeatSelectionType switch
                {
                    "VIEW_SEAT" or "SELECT_SEAT" => true,
                    "SEAT_ASSIGNED_AT_GATE" or "SEAT_ASSIGNED_AT_CHECKIN" => false,
                    _ => true
                },
                OperationalStatus = segment.OperationalStatus != null ? (OperationalStatus)segment.OperationalStatus : null,
                CheckedIn = segment.CheckedIn,
                OtherAirlineOperatedHelpTitle = otherAirlineOperatedHelpTitle,
                OtherAirlineOperatedHelpText = otherAirlineOperatedHelpText
            };
        }

        private static (DateTime? Open, DateTime? Close) GetPopsTimings(string? operatingCarrierCode, DateTime? scheduledDepartureUtc)
        {
            if (!operatingCarrierCode.Is(Constants.Application.VirginAtlanticCarrierCode) || !scheduledDepartureUtc.HasValue)
                return (null, null);

            var popsOpenDaysBeforeDeparture = System.Math.Abs(Common.Helper.ConfigReader.Try(Constants.Environment.PopsOpenDaysBeforeDepartureKey, Constants.Environment.DefaultPopsOpenDaysBeforeDeparture));
            var popsCloseDaysBeforeDeparture = System.Math.Abs(Common.Helper.ConfigReader.Try(Constants.Environment.PopsCloseDaysBeforeDepartureKey, Constants.Environment.DefaultPopsCloseDaysBeforeDeparture));

            return (
                scheduledDepartureUtc.Value.AddDays(-popsOpenDaysBeforeDeparture),
                scheduledDepartureUtc.Value.AddDays(-popsCloseDaysBeforeDeparture));
        }

        private static (string? Title, string? Text) GetOperatingCarrierHelpInfo(string? operatingCarrierCode, string? operatingCarrierName, IEnumerable<AirlineRefData> airlineDetails)
        {
            if (operatingCarrierCode.IsNullOrWhiteSpace() || operatingCarrierCode.Is(Constants.Application.VirginAtlanticCarrierCode))
            {
                return (null, null);
            }

            var airlineName =
                airlineDetails?
                    .FirstOrDefault(a => a.CarrierCode == operatingCarrierCode)
                    ?.CarrierName
                ?? operatingCarrierName ?? string.Empty;

            string title = $"This flight is with {airlineName}";

            return operatingCarrierCode switch
            {
                "KE" => ("This flight is with Korean Air", ConfigReader.Get(Constants.Environment.KoreanAirlineOperatedHelpText)),

                _ => (title, ConfigReader.Get(Constants.Environment.OtherAirlineOperatedHelpText))
            };
        }
        public static explicit operator Segment((Infrastructure.Models.HolidaysMS.TripsCore.Segment segment, IEnumerable<AirportRefData> airportRefData) source)
            => (Segment)(source.segment, source.airportRefData, Enumerable.Empty<AirlineRefData>());

        public static explicit operator Segment((Infrastructure.Models.HolidaysMS.TripsCore.Segment segment, IEnumerable<AirportRefData> airportRefData, IEnumerable<AirlineRefData> airlineRefData) source)
            => (Segment)(source.segment, source.airportRefData, source.airlineRefData, true);

        public static explicit operator Segment((Infrastructure.Models.HolidaysMS.TripsCore.Segment segment, IEnumerable<AirportRefData> airportRefData, IEnumerable<AirlineRefData> airlineRefData, bool isSupportedFlight) source)
        {
            var (segment, airportRefData, airlineRefData, isSupportedFlight) = source;

            if (segment == null) return null;

            DateTime? scheduledDepartureUtc = segment.ScheduledDepartureUtcDateTime == default
                ? null
                : segment.ScheduledDepartureUtcDateTime;
            var (popsOpenDateTimeUtc, popsCloseDateTimeUtc) = GetPopsTimings(segment.OperatingCarrier?.CarrierCode, scheduledDepartureUtc);
            var (otherAirlineOperatedHelpTitle, otherAirlineOperatedHelpText) = GetOperatingCarrierHelpInfo(segment.OperatingCarrier?.CarrierCode, segment.OperatingCarrier?.CarrierName, airlineRefData);

            return new Segment()
            {
                Destination = (AirportDetails)(segment.Destination, airportRefData),
                FlightSegmentId = segment.Id,
                Id = segment.Id,
                PnrLocator = segment.PnrLocator,
                MarketingCarrier = (CarrierDetails)(segment.MarketingCarrier, airlineRefData),
                OperatingCarrier = (CarrierDetails)(segment.OperatingCarrier, airlineRefData),
                Legs = segment.Legs?.Select(x => (Leg)(x, airportRefData, airlineRefData)),
                Origin = (AirportDetails)(segment.Origin, airportRefData),
                ScheduledDepartureLocal = DateTimeHelper.ToLocal(segment.ScheduledDepartureLocal),
                ScheduledArrivalLocal = DateTimeHelper.ToLocal(segment.ScheduledArrivalLocal),
                PopsOpenDateTimeUtc = popsOpenDateTimeUtc,
                PopsCloseDateTimeUtc = popsCloseDateTimeUtc,
                OtherAirlineOperatedHelpTitle = otherAirlineOperatedHelpTitle,
                OtherAirlineOperatedHelpText = otherAirlineOperatedHelpText,
                ManageFlightLink = GetManageFlightLink(segment.OperatingCarrier?.CarrierCode ?? segment.MarketingCarrier?.CarrierCode, airlineRefData, isSupportedFlight)
            };
        }

        private static string? GetManageFlightLink(string? carrierCode, IEnumerable<AirlineRefData> airlineRefData, bool isSupportedFlight)
            => isSupportedFlight || carrierCode.IsNullOrWhiteSpace()
                ? null
                : airlineRefData?.FirstOrDefault(airline => string.Equals(airline.CarrierCode, carrierCode, StringComparison.OrdinalIgnoreCase))?.Url;
    }
}
