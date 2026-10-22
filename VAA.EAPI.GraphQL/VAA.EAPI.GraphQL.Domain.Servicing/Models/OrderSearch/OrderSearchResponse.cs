namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class OrderSearchResponse
    {
        public List<OriginDestinationAssociations>? OriginDestinationAssociations { get; set; }
        public List<CustomerDetails>? CustomerDetails { get; set; }
        public List<OrderInstructions>? OrderInstructions { get; set; }
        public List<SegmentDetails>? SegmentDetails { get; set; }
        public PointOfSale? PointOfSale { get; set; }
        public CreationDateTime? CreationDateTime { get; set; }
        public OrderNumber? OrderNumber { get; set; }
        public BookingReferences? BookingReferences { get; set; }
        public PaymentDetails? PaymentDetails { get; set; }
        public List<OrderItems>? OrderItems { get; set; }
        public string? OrderStatus { get; set; }
    }
}
