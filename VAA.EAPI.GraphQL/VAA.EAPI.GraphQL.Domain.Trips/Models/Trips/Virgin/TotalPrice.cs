using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsTotalPrice")]
    public class TotalPrice
    {
#warning For backwards compatibility has to stay as int
        public int? Amount { get; set; }
        public string? Currency { get; set; }

        public static explicit operator TotalPrice(Infrastructure.Models.TripMS.TotalPrice source)
        {
            if (source == null)
                return null;

            return new TotalPrice()
            {
                Amount = (int)source.Amount,
                Currency = source.Currency
            };
        }
    }
}
