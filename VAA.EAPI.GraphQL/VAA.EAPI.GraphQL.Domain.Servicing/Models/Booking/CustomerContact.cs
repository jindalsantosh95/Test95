using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking
{
    public class CustomerContact
    {
        [GraphQLDescription("Customer Email")]
        public string? Email { get; set; }
        [GraphQLDescription("Customer PhoneNo")]
        public string? Phone { get; set; }
        [GraphQLDescription("Country Code")]
        public string? Countrycode { get; set; }
    }
}
