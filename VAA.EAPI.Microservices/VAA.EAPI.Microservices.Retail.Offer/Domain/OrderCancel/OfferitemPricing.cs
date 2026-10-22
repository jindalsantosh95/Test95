using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel.Enums;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel
{
    public class OfferitemPricing
    {
        public ReshoppingActionType ReshoppingAction { get; set; }
        public IEnumerable<Price> SalePrice { get; set; }
        public IEnumerable<Price> OriginalSalePrice { get; set; }
        public IEnumerable<Price> SalePriceDifference { get; set; }
        public IEnumerable<PenaltyPrice> PenaltyPrices { get; set; }
        public IEnumerable<RefundPrice> RefundPrice { get; set; }
    }
}
