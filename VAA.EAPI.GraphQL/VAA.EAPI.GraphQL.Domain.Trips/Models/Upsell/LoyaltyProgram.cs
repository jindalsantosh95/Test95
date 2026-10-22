using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [GraphQLName("UpsellLoyaltyProgram")]
    public class LoyaltyProgram
    {
        public string Name { get; set; }
        public IEnumerable<Carrier> Owner { get; set; }

        public static explicit operator LoyaltyProgram(Infrastructure.Models.UpsellOffer.LoyaltyProgram source)
        {
            if (source == null) return null;

            return new LoyaltyProgram
            {
                Name = source.Name,
                Owner = source.Owner?.Select(c => (Carrier)c)
            };
        }
    }
}

