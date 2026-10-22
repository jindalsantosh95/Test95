using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel
{
    public class ReshopOffer
    {
        public string OfferId { get; set; }
        public string ExistngOrderIdRef { get; set; }
        public IEnumerable<Price> SalePrice { get; set; }
        public IEnumerable<Price> OriginalSalePrice { get; set; }
        public IEnumerable<Price> SalePriceDifference { get; set; }
        public IEnumerable<ReshopOfferItem> OfferItems { get; set; }
        public string OfferExpirationTimeLimit { get; set; }
    }
}
