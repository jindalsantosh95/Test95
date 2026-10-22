namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class TripSummary
    {
        public IEnumerable<SliceDetails> SliceDetails { get; set; } = Enumerable.Empty<SliceDetails>();
        public string Currency { get; set; }
        public decimal TotalPrice => SliceDetails.Sum(slc => slc.selectedPrice);
        public string? TotalAwardPoints { get; set; }
    }
}
