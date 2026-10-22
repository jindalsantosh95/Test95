namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class FlightSegment
    {
        public string ArrivalDateTime { get; set; }
        public string DepartureDateTime { get; set; }
        public string FlightNumber { get; set; }
        public string NumberInParty { get; set; }
        public string ResBookDesigCode { get; set; }
        public string Status { get; set; }
        public DestinationLocation DestinationLocation { get; set; }
        public MarketingAirline MarketingAirline { get; set; }
        public string MarriageGrp { get; set; }
        public OriginLocation OriginLocation { get; set; }
    }
}
