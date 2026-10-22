using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.Customer
{
    [GraphQLName("GraphqlTripsCustomerInfo")]
    public class CustomerInfo
    {
        public string CustId { get; set; }
        public string Title { get; set; }
        public string Ptc { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public Contacts Contacts { get; set; }
        public Address Addresses { get; set; }
        public string LinkedInfantId { get; set; }
    }
}
