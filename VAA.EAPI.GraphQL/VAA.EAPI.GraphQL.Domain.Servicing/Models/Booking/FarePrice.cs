using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking
{
    public class FarePrice
    {
        [GraphQLDescription("Fare Price Details")]
        public List<FarePriceDetail>? FarePriceDetails { get; set; }
        [GraphQLDescription("Segment Details")]
        public List<DtiSegmentInfo>? DtiSegmentInfo { get; set; }
    }
}
