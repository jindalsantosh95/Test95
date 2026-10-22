using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [GraphQLName("UpsellCarrier")]
    public class Carrier
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator Carrier(Infrastructure.Models.UpsellOffer.Carrier source)
        {
            if (source == null) return null;

            return new Carrier()
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}

