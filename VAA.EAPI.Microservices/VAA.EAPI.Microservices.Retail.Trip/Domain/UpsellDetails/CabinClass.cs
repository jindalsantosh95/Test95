

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class CabinClass
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public IEnumerable<Amenity> Amenities { get; set; }

        public static explicit operator CabinClass(Infrastructure.Proxy.DeltaUpsellOffers.CabinClass source)
        {
            if (source == null) return null;

            return new CabinClass
            {
                Code = source.Code,
                Name = source.Name,
                ProductCode = source.ProductCode,
                ProductName = source.ProductName,
                Amenities = source.Amenities?.Select(amenity => (Amenity)amenity)
            };
        }
    }
}

