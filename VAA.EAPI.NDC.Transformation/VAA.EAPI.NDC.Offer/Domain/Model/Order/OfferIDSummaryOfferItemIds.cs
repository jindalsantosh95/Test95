using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OfferIDSummaryOfferItemIds
    {
        public string OrderItemId { get; set; }
        public IEnumerable<string> CustomerIds { get; set; }
        public IEnumerable<string> ProductCodes { get; set; }
    }
}
