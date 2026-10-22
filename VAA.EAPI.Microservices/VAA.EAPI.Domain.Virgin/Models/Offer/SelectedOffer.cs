using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Domain.Virgin.Models.Offer
{
    public class SelectedOffer
    {
        public string OfferId { get; set; }
        public List<Price> SalePrice { get; set; }
        public List<OfferItem> OfferItems { get; set; }
    }

}
