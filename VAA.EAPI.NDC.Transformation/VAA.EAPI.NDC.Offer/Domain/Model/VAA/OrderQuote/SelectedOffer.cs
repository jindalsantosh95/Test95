using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote
{
    public partial class SelectedOffer
    {
        public string ShoppingRefID { get; set; }

        public string OfferRefID { get; set; }

        public string FareFamily { get; set; }

        public IEnumerable<PricedOfferItem> OfferItems { get; set; }

        public string OwnerCode { get; set; }


    }
}
