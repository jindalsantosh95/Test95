namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    [GraphQLName("AddressSearch")]
    public class Address
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
    }
}
