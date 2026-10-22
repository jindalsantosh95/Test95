
using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{

    [GraphQLName("SummaryFreeBaggageAllowance")]
    public class FreeBaggageAllowance
    {
        public int AllowedBagCnt { get; set; }
        public Weight? Weight { get; set; }
        public DimensionAllowance? DimensionAllowance { get; set; }

    }
}
