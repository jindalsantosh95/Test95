using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OfferIDSummary
    {
        public string OfferId { get; set; }
        public IEnumerable<OfferIDSummaryOfferItemIds> OfferItemIds { get; set; }


    }
}
