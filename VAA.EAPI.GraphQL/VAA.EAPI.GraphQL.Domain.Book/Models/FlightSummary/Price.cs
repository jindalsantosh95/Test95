using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryPrice")]
    public class Price
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public decimal Tax { get; set; }
        public decimal? PriceDifference { get; set; }
        public string? PriceDifferenceSign { get; set; }
        public decimal AmountIncludingTax { get; set; }
        public string? AwardPoints { get; set; }
        public string? VoucherPoints { get; set; }
        public string? PointsToPay { get; set; }
    }
}
