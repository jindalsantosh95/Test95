
namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions
{
    public class CreditSector
    {
        public string CabinClass { get; set; }
        public string DestinationAirport { get; set; }
        public string FlownBookingClass { get; set; }
        public string MarketingBookingClass { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingAirlineAuthNumber { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginAirport { get; set; }
        public int Points { get; set; }
        public string SectorCityPair { get; set; }
        public string ServiceClass { get; set; }
        public string TicketNumber { get; set; }
    }
}
