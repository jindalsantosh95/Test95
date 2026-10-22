namespace VAA.EAPI.GraphQL.Infrastructure.Entities
{
    public class CabinClassDetailsRequest
    {
        public string OperatedAirlineCode { get; set; }
        public string OperatedBookingClass { get; set; }
        public string MarketingAirlineCode { get; set; }
        public string MarketingBookingClass { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string FlightActivityDate { get; set; }
    }
}
