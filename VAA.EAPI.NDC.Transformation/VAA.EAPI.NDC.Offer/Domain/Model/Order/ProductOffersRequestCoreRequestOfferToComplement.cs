using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class ProductOffersRequestCoreRequestOfferToComplement
    {
        public OfferNumber OfferId { get; set; }
        public IEnumerable<OfferItemNumber> OfferItemIds { get; set; }


    }
}
