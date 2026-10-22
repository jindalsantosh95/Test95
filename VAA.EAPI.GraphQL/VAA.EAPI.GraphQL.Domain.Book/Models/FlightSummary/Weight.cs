using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryWeight")]
    public class Weight
    {
        public int UnitOfMeasureCnt { get; set; }
        public string? UnitOfMeasure { get; set; }




    }
}