using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking
{
    public class VaaBookingCreationDateTime
    {
        [GraphQLDescription("Booking Creation Local DateTime")]
        public string? LocalDateTime { get; set; }
        [GraphQLDescription("Booking Creation UTC Datetime")]
        public string? UtcDateTime { get; set; }
    }
}
