namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class Distance
    {
        public Carrier Carrier { get; set; }
        public string DestinationAirportCode { get; set; }
        public string EndorsementText { get; set; }
        public string ExtendedFareCode { get; set; }
        public string OriginAirportCode { get; set; }
    }
}
