

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class Amenity
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator Amenity(Infrastructure.Proxy.DeltaUpsellOffers.Amenity source)
        {
            if (source == null) return null;

            return new Amenity
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}

