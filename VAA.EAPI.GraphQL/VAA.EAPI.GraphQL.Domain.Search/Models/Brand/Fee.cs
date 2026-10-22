namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    public class Fee
    {
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public decimal? Percentage { get; set; }
        public string? Higher_Lower { get; set; }
    }
}