namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class SearchSummaryRequest
    {
        public Guid BasketId { get; set; }
        public string OfferId { get; set; }
        [GraphQLDeprecated("CurrentTripIndexId should be removed. Summary is only ever called for the final slice.")]
        public int? CurrentTripIndexId { get; set; }
        public string? SummaryLink { get; set; }
    }
}