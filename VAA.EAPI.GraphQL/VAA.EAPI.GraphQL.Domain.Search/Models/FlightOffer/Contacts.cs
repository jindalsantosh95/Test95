namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    [GraphQLName("ContactsSearch")]
    public class Contacts
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Countrycode { get; set; }
    }
}
