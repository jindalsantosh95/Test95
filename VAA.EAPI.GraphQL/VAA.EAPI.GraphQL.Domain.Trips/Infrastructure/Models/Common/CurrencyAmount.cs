using HotChocolate;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.Common
{
    [GraphQLName("GraphqlTripsCurrencyAmount")]
    public class CurrencyAmount
    {
        public string MonetaryAmount { get; set; }
        public string BaseCurrency { get; set; }
        public string CurrencyId { get; set; }
        public int DecimalPrecisionCount { get; set; }
        public decimal PriceDifference { get; set; }
        public string PriceDifferenceSign { get; set; }
    }
}