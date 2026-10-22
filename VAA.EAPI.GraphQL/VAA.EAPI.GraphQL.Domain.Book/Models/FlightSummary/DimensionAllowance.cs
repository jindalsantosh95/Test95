using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryDimensionAllowance")]
    public class DimensionAllowance
    {
        public string? Height { get; set; }
        public string? Width { get; set; }
        public string? Linear { get; set; }
        public string? Length { get; set; }
        public string? UnitOfMeasure { get; set; }


    }
}
