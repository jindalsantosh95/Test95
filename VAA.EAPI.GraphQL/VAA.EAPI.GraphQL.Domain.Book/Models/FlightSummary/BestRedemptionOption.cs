using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("BookBestRedemptionOption")]
    public sealed class BestRedemptionOption
    {
        public int OptionId { get; set; }
        public string? OptionName { get; set; }
        public string? RedemptionType { get; set; }
        public string? SelectionReason { get; set; }
    }
}
