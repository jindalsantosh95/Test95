using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Ticket;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Order
{
    public class OrderItems
    {
        public string OrderItemId { get; set; }
        public string OwnerAirline { get; set; }
        public string OfferIdReference { get; set; }
        public string OfferItemIdReference { get; set; }
        public IEnumerable<OrderService> Services { get; set; }
        public IEnumerable<Price> PriceDetails { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
        public string PNRReferences { get; set; }
        public State ItemStatus { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
    }
}
