
namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.UpsellOffer
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

