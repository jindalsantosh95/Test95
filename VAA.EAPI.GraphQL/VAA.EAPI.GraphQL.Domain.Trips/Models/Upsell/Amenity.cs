using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [GraphQLName("UpsellAmenity")]
    public class Amenity
    {
        public string Name { get; set; }

        public static explicit operator Amenity(Infrastructure.Models.UpsellOffer.Amenity source)
        {
            if (source == null) return null;

            return new Amenity()
            {
                Name = source.Name
            };
        }
    }
}

