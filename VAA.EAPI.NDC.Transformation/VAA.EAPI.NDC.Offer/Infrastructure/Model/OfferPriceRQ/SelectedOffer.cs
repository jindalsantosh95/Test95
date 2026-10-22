using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferPriceRQ
{
    public class SelectedOffer
    {
        public string ShoppingRefID { get; set; }
        public string OfferRefID { get; set; }
        public string FareFamily { get; set; }
        public IEnumerable<PricedOfferItem> OfferItems { get; set; }
    }
}
