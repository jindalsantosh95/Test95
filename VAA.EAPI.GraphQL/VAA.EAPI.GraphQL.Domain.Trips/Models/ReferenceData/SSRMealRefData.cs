namespace VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData
{
    public class SSRMealRefData
    {
        public string? Category { get; set; }
        public string? CategoryTitle { get; set; }
        public IEnumerable<CategoryDetail>? CategoryDetails { get; set; }
    }
}