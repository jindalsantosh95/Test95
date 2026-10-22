using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Brand
{
    public class FlightSegment
    {
        public string OriginAirportCode { get; set; }
        public string DestAirportCode { get; set; }
        public string SchedDepartLocalTs { get; set; }
        public string OperatingFlightNum { get; set; }
        public string OperatingCos { get; set; }
        public Aircraft Aircraft { get; set; }
        public OperatingCarrier OperatingCarrier { get; set; }
        public object MarketingCarrier { get; set; }
        public object PublishedCarrier { get; set; }
        public int Id { get; set; }
        public string MarketingFlightNum { get; set; }
        public string MarketingCos { get; set; }
        public List<FlightLeg> FlightLeg { get; set; }
    }
}
