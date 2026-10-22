using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking
{
    public class FrequentFlyer
    {
        [GraphQLDescription("FC Number")]
        public string? AccountNumber { get; set; }
        [GraphQLDescription("Airline Code")]
        public string? AirlineCode { get; set; }
        [GraphQLDescription("First Name")]
        public string? FirstName { get; set; }
        [GraphQLDescription("Last Name")]
        public string? LastName { get; set; }
        [GraphQLDescription("Osi Number")]
        public string? OsiNumber { get; set; }
    }
}
