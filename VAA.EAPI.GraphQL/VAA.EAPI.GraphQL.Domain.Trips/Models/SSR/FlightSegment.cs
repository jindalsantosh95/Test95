using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.SSR
{
    [GraphQLName("GraphqlSSRFlightSegment")]
    public class FlightSegment
    {
        public string? SegmentId { get; set; }
        public int? Ordinal { get; set; }
        public IEnumerable<Airline>? OperatingAirlines { get; set; }
        public string? FlightNumber { get; set; }
        public MarketingFlight? MarketingFlight { get; set; }
        public IEnumerable<Airport>? DepartureAirport { get; set; }
        public IEnumerable<Airport>? ArrivalAirport { get; set; }
        public bool? CheckinStatus { get; set; }
        public IEnumerable<FlightLeg>? FlightLegs { get; set; }

        public static explicit operator FlightSegment((Infrastructure.Models.SSRMS.FlightSegment segment, IEnumerable<AirportRefData> airports) source)
        {
            var (segment, airports) = source;
            if (segment == null)
                return null;

            return new()
            {
                ArrivalAirport = segment.ArrivalAirport.Select(a => (Airport)(a, airports)),
                CheckinStatus = segment.CheckinStatus,
                DepartureAirport = segment.DepartureAirport.Select(d => (Airport)(d, airports)),
                FlightLegs = segment.FlightLegs.Select(fl => (FlightLeg)fl),
                FlightNumber = segment.FlightNumber,
                MarketingFlight = (MarketingFlight)segment.MarketingFlight,
                OperatingAirlines = segment.OperatingAirlines.Select(oa => (Airline)oa),
                Ordinal = segment.Ordinal,
                SegmentId = segment.SegmentId.ToString()
            };
        }
    }
}