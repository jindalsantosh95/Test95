namespace VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS
{
    public class Destination
    {
        public string Code { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string City { get; internal set; }
    }
}