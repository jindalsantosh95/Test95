

using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryPaidBaggage")]
    public class PaidBaggage
    {
        public Weight? Weight { get; set; }
        public DimensionAllowance? DimensionAllowance { get; set; }
        public Price Price { get; set; }


    }
}
