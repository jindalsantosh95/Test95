namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class MarketingSegment
    {
        public string OperationalSuffix { get; set; }
        public string FlightNum { get; set; }
        public IEnumerable<Carrier> Carrier { get; set; }
        public string FareBasisCode { get; set; }
    }


}
