using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking
{
    public class GetBookingResponse
    {
        [GraphQLDescription("Customer Details")]
        public List<VaaCustomerDetail>? VaaCustomerDetails { get; set; }
        [GraphQLDescription("Customer's latest past/future segment details")]
        public List<VaaBookingDetail>? VaaBookingDetails { get; set; }
    }
}
