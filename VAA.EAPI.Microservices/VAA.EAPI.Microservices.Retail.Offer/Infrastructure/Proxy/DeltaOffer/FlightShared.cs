namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FlightShared
    {
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string OperatingFlightNum { get; set; }
        public string ScheduledDepartureLocalTs { get; set; }
    }
}
