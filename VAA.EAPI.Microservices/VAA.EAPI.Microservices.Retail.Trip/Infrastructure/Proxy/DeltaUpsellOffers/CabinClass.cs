namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    public class CabinClass
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public IEnumerable<Amenity> Amenities { get; set; }
    }
}
