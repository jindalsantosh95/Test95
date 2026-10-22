namespace VAA.EAPI.Microservices.Retail.Insurance.Domain
{
    public sealed class InsuranceQuoteResponse
    {
        public Status Status { get; set; }
        public IEnumerable<Product>? Products { get; set; }
        public IEnumerable<Location>? Locations { get; set; }
        public TransactionDetails TransactionDetails { get; set; }
    }
}

