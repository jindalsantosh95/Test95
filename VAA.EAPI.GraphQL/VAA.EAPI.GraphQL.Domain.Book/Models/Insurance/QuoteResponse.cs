namespace VAA.EAPI.GraphQL.Domain.Book.Models.Insurance
{
    public class QuoteResponse
    {
        public Status Status { get; set; }
        public IEnumerable<Offer> Offers { get; set; }
        public TransactionDetails TransactionDetails { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }
}