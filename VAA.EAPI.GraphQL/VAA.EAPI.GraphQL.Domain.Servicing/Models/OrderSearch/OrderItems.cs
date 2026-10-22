
namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class OrderItems
    {
        public string? OrderItemId { get; set; }
        public string? OwnerAirline { get; set; }
        public string? OfferIdReference { get; set; }
        public string? OfferItemIdReference { get; set; }
        public List<Service>? Services { get; set; }
        public List<PriceDetail>? PriceDetails { get; set; }
        public List<AccountableDocumentRef>? AccountableDocumentRefs { get; set; }
        public string? PNRReferences { get; set; }
        public ItemStatus? ItemStatus { get; set; }
        public List<OrderItemPenalty>? OrderItemPenalties { get; set; }
        public List<PaymentDetails>? PaymentDetails { get; set; }
    }
}
