using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Ticket;

namespace VAA.EAPI.Domain.Virgin.Models.Order
{
    public class OrderItems
    {
        public string OrderItemId { get; set; }
        public string ExistingOrderItemId { get; set; }
        public string OwnerAirline { get; set; }
        public string OCNTimestamp { get; set; }
        public string OfferIdReference { get; set; }
        public string OfferItemIdReference { get; set; }
        public IEnumerable<OrderService> Services { get; set; }
        public IEnumerable<Price> PriceDetails { get; set; }
        public IEnumerable<Price> SalePriceDifference { get; set; }
        public IEnumerable<Price> OriginalSalePrice { get; set; }
        public AddCollectPrice AddCollectPrice { get; set; }
        public IEnumerable<RefundPrice> RefundPrice { get; set; }
        public IEnumerable<PenaltyPrice> PenaltyPrices { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
        public string PNRReferences { get; set; }
        public State ItemStatus { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public IEnumerable<OrderItemError> Errors { get; set; }
        public FareComponent FareComponents { get; set; }
        public IEnumerable<string> SellerFollowUpAction { get; set; }
    }


}
