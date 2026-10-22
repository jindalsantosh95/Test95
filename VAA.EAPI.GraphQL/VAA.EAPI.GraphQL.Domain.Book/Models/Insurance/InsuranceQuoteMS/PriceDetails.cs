namespace VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS
{
    public class PriceDetails
    {
        public bool IsAvailable { get; set; }
        public bool IsIncluded { get; set; }
        public double Price { get; set; }
        public IEnumerable<Sequence>? Sequences { get; set; }
    }
}