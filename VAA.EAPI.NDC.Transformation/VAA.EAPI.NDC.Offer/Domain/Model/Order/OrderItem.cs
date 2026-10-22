using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OrderItem
    {
        public OrderItemNumber OrderItemId { get; set; }
        public OrderItemSupplierReference SupplierReference { get; set; }
        public OfferNumber OfferIdReference { get; set; }
        public OfferItemNumber OfferItemIdReference { get; set; }
        public IEnumerable<Service> Services { get; set; }
        public PriceDetail PriceDetails { get; set; }
        public IEnumerable<AccountableDocumentReference> AccountableDocumentRefs { get; set; }
        public IEnumerable<BookingReference> PNRReferences { get; set; }
        public ItemStatus LineItemStatus { get; set; }
        public Link OrderItemFinalisation { get; set; }
        public IEnumerable<Penalty> OrderItemPenalties { get; set; }
        public IEnumerable<Disclosure> OrderItemDisclosures { get; set; }
        public TimeLimits OrderItemTimeLimits { get; set; }
        public bool? Visibility { get; set; }

    }
}
