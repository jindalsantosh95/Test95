namespace VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS
{
    public sealed class InsuranceQuoteRequest
    {
        public CustomerAddress CustomerAddress { get; set; }
        public IEnumerable<Customer> Customers { get; set; }
        public Gadget Gadget { get; set; }
        public Partner Partner { get; set; }
        public RequestDetail RequestDetail { get; set; }
        public Travel Travel { get; set; }
    }
}
