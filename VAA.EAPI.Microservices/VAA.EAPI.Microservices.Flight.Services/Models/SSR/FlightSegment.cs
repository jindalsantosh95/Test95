using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Models.SSR
{
    public class FlightSegment
    {
        public int SegmentId { get; set; }
        public int Ordinal { get; set; }
        public IEnumerable<Airline> OperatingAirlines { get; set; }
        public string FlightNumber { get; set; }
        public MarketingFlight MarketingFlight { get; set; }
        public IEnumerable<Airport> DepartureAirport { get; set; }
        public IEnumerable<Airport> ArrivalAirport { get; set; }
        public bool CheckinStatus { get; set; }
        public IEnumerable<FlightLeg> FlightLegs { get; set; }

        public static explicit operator FlightSegment(DatedOperatingSegment sourceSegment)
        {
            if (sourceSegment == null)
                return null;

            return new FlightSegment
            {
                SegmentId = sourceSegment.SegmentId,
                Ordinal = sourceSegment.Ordinal,
                OperatingAirlines = sourceSegment.OperatingCarrier?.Select(c => (Airline)c),
                FlightNumber = sourceSegment.FlightNum,
                MarketingFlight = (MarketingFlight)sourceSegment.MarketingSegment,
                DepartureAirport = sourceSegment.OriginStation?.Select(s => (Airport)s),
                ArrivalAirport = sourceSegment.DestinationStation?.Select(s => (Airport)s),
                CheckinStatus = sourceSegment.CheckedIn,
                FlightLegs = sourceSegment.Legs?.Select(l => (FlightLeg)l)
            };
        }
    }

}
