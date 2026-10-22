using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("tripsPaymentAmount")]
    public class CurrencyAmount
    {
        public decimal MonetaryAmount { get; set; }
        public string BaseCurrency { get; set; }
    }
}