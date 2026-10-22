using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Ticket;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.InRule
{
    public class OrderItem
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
        public IEnumerable<OrderItemError> Errors { get; set; }
        public FareComponent FareComponents { get; set; }
        public IEnumerable<string> SellerFollowUpAction { get; set; }

        public static explicit operator OrderItem(OrderItems source) =>
            source is null
                ? null
                : new OrderItem
                {
                    OrderItemId = source.OrderItemId,
                    ExistingOrderItemId = source.ExistingOrderItemId,
                    OwnerAirline = source.OwnerAirline,
                    OCNTimestamp = source.OCNTimestamp,
                    OfferIdReference = source.OfferIdReference,
                    OfferItemIdReference = source.OfferItemIdReference,
                    Services = source.Services,
                    PriceDetails = source.PriceDetails,
                    SalePriceDifference = source.SalePriceDifference,
                    OriginalSalePrice = source.OriginalSalePrice,
                    AddCollectPrice = source.AddCollectPrice,
                    RefundPrice = source.RefundPrice,
                    PenaltyPrices = source.PenaltyPrices,
                    AccountableDocumentRefs = source.AccountableDocumentRefs,
                    PNRReferences = source.PNRReferences,
                    ItemStatus = source.ItemStatus,
                    Errors = source.Errors,
                    FareComponents = source.FareComponents,
                    SellerFollowUpAction = source.SellerFollowUpAction
                };
    }
}
