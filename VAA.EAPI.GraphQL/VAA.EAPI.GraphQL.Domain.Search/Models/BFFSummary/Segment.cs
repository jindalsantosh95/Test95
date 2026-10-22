using NodaTime;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    [GraphQLName("Segment_BFF")]
    public class Segment
    {
        public IEnumerable<Metal>? Metal { get; set; }
        public Airline? Airline { get; set; }
        public string? FlightNumber { get; set; }
        public string? OperatingFlightNumber { get; set; }
        public Airline? OperatingAirline { get; set; }
        public Origin? Origin { get; set; }
        public Destination? Destination { get; set; }
        public string? Duration { get; set; }
        public LocalDateTime? Departure { get; set; }
        public LocalDateTime? Arrival { get; set; }
        public int StopCount { get; set; }
        public string? Connection { get; set; }
        public IEnumerable<Leg>? Legs { get; set; }
        public string? BookingClass { get; set; }
        public string? FareBasisCode { get; set; }
        public string? DominantFareProduct { get; set; }
        public bool? IsSaverFare { get; set; }
        public bool? PendingGovtApproval { get; set; }

        public Segment(FlightOffer.Segment fs, OfferService service, int stopCount, IEnumerable<CabinInfo> cabinInfo, IEnumerable<AircraftInfo> aircraftInfo, IEnumerable<AirlineInfo> airlineInfo, IEnumerable<AirportInfo> airportInfo)
        {
            var fareSegment = service.FareDetails?.FareSegments?.Where(f => f.FlightSegmentRefID == fs.SegmentId).FirstOrDefault();
            var dominantCabin = cabinInfo.FirstOrDefault(ci => ci.BrandID == fareSegment?.CabinName);

            Metal = [new Metal(fs.AirCraft, aircraftInfo)];
            Airline = (Airline)airlineInfo.FirstOrDefault(ai => ai.CarrierCode == fs.MarketingCarrier);
            FlightNumber = $"{fs.MarketingCarrier}{fs.MarketingFlightNumber}";
            OperatingAirline = (Airline)airlineInfo.FirstOrDefault(ai => ai.CarrierCode == fs.OperatingCarrier);
            OperatingFlightNumber = $"{fs.OperatingCarrier}{fs.OperatingFlightNumber}";
            Origin = (Origin)(fs.OriginAirportCode, airportInfo);
            Destination = (Destination)(fs.DestinationAirportCode, airportInfo);
            Duration = Helpers.DateTimeHelper.ConvertColonDurationToIso(fs.FlightDuration);
            Departure = Common.Helper.DateTimeHelper.ParseLocalDateTime(fs.ScheduledDepartureTime.LocalDateTime);
            Arrival = Common.Helper.DateTimeHelper.ParseLocalDateTime(fs.ScheduledArrivalTime.LocalDateTime);
            StopCount = GetStopCount(fs.ConnectionOrStopOver, stopCount);
            Connection = string.IsNullOrEmpty(fs.ConnectionDuration) ? null : Helpers.DateTimeHelper.ConvertColonDurationToIso(fs.ConnectionDuration);
            Legs = fs.Legs.Select(leg => (Leg)(leg, airportInfo, airlineInfo));
            BookingClass = fareSegment?.BookingClass;
            FareBasisCode = fareSegment?.FareBasisCode ?? service.FareDetails?.FareBasisCode;
            DominantFareProduct = dominantCabin?.BrandName ?? service.FareDetails?.DominantFareProduct;
            IsSaverFare = fareSegment?.IsSaverFare;
            PendingGovtApproval = fs.PendingGovtApproval;
        }

        private static int GetStopCount(string connectionOrStpOver, int stopCount)
        {
            return connectionOrStpOver == "0" ? 0 : ++stopCount;
        }
    }
}