using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking
{
    public class VaaCustomerDetail
    {
        [GraphQLDescription("VAA SCV Id of customer")]
        public string? VaaScvId { get; set; }
        [GraphQLDescription("Contact type text")]
        public string? ContactTypeText { get; set; }
        [GraphQLDescription("Title")]
        public string? Title { get; set; }
        [GraphQLDescription("Customer Id")]
        public string? CustomerId { get; set; }
        [GraphQLDescription("First Name")]
        public string? FirstName { get; set; }
        [GraphQLDescription("Last Name")]
        public string? LastName { get; set; }
        [GraphQLDescription("First Name Number")]
        public string? FirstNameNumber { get; set; }
        [GraphQLDescription("Last Name Number")]
        public string? LastNameNumber { get; set; }
        [GraphQLDescription("Booking Type")]
        public string? BookingType { get; set; }
        [GraphQLDescription("Customer frequent flyer details")]
        public FrequentFlyer? FrequentFlyer { get; set; }
        [GraphQLDescription("Customer Contact details")]
        public List<CustomerContact>? CustomerContacts { get; set; }
    }
}
