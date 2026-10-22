using VAA.EAPI.GraphQL.Domain.Book.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Order
{
    public class OrderService
    {
        public string ServiceId { get; set; }
        public Product Product { get; set; }
        public ProductSegmentAssociation ProductSegmentAssociation { get; set; }
        public string CurrentSeatAssignment { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
        public IEnumerable<string> OrderInstructionRefs { get; set; }
        public IEnumerable<Price> PriceDetails { get; set; }
        public State Status { get; set; }
        public IEnumerable<string> CustomerRefIds { get; set; }
        public IEnumerable<string> OriginDestinationRefIds { get; set; }
        public IEnumerable<string> SegmentRefIds { get; set; }
        public SeatInformation SeatInformation { get; set; }
    }
}