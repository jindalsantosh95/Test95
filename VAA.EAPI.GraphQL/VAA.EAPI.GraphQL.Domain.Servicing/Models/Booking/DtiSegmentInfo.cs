using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking
{
    public class DtiSegmentInfo
    {
        [GraphQLDescription("Segment booking number")]
        public string? BookNumber { get; set; }
        [GraphQLDescription("Segment Carrier")]
        public string? Carrier { get; set; }
        [GraphQLDescription("Fare Class Code")]
        public string? FareBasisFareClassCode { get; set; }
    }
}
