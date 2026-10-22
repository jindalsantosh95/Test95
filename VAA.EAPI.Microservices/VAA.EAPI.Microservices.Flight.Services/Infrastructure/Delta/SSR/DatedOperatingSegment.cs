using System.Collections.Generic;
using VAA.EAPI.Microservices.Flight.Services.Models.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    public class DatedOperatingSegment
    {
        public int SegmentId { get; set; }
        public int Ordinal { get; set; }
        public IEnumerable<Carrier> OperatingCarrier { get; set; }
        public string FlightNum { get; set; }
        public MarketingSegment MarketingSegment { get; set; }
        public IEnumerable<Station> OriginStation { get; set; }
        public IEnumerable<Station> DestinationStation { get; set; }
        public bool CheckedIn { get; set; }
        public string ActionCode { get; set; }
        public IEnumerable<Leg> Legs { get; set; }

        public static explicit operator DatedOperatingSegment(FlightSegment flightSegment)
        {
            if (flightSegment == null)
                return null;

            return new DatedOperatingSegment
            {
                SegmentId = flightSegment.SegmentId,
            };
        }
    }
}