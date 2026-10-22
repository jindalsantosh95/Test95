using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class SelectedOffer
    {
        public string ShoppingRefID { get; set; }
        public string OfferRefID { get; set; }
        public List<PricedOfferItem> OfferItems { get; set; }
    }
}
