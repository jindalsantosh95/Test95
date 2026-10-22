using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS
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
