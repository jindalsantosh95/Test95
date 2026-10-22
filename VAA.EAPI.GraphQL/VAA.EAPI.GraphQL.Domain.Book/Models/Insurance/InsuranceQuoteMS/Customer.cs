namespace VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS
{
    public class Customer
    {
        public string CustomerId { get; set; }
        public string CustomerType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<Membership> Memberships { get; set; }
        public int Age { get; set; }
    }
}