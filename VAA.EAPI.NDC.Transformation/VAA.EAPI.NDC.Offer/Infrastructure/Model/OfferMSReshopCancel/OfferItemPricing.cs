using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMSReshopCancel
{
    public class OfferItemPricing
    {
        public ReshoppingActionType ReshoppingAction { get; set; }
        public IEnumerable<Price> SalePrice { get; set; }
        public IEnumerable<RefundPrice> RefundPrice { get; set; }
        public IEnumerable<Price> OriginalSalePrice { get; set; }
        public IEnumerable<Price> SalePriceDifference { get; set; }       
    }
}
