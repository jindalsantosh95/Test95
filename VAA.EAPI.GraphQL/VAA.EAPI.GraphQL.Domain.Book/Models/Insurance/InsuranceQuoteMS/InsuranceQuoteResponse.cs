namespace VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS
{
    public sealed class InsuranceQuoteResponse
    {
        public Status Status { get; set; }
        public IEnumerable<Product>? Products { get; set; }
        public IEnumerable<Location>? Locations { get; set; }
        public TransactionDetails TransactionDetails { get; set; }
    }
}

