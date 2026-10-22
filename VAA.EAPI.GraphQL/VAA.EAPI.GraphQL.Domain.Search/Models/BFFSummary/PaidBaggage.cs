using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    [GraphQLName("PaidBaggage_BFF")]
    public class PaidBaggage
    {
        public Weight? Weight { get; set; }
        public DimensionAllowance? DimensionAllowance { get; set; }
        public Price Price { get; set; }

        public static explicit operator PaidBaggage(PaidBag? source)
        {
            if (source?.Fee?.Amount is null)
                return new PaidBaggage { };

            return new PaidBaggage()
            {
                Price = new Price { Amount = source.Fee.Amount.Value, Currency = source.Fee.Currency },
                Weight = (Weight?)source.Weight,
                DimensionAllowance = (DimensionAllowance?)source.Size
            };
        }
    }
}
