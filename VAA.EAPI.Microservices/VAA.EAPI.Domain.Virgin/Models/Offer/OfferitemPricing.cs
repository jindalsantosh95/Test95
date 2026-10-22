using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Domain.Virgin.Models.Offer
{
    public class OfferitemPricing
    {
        public string FareCalculationText { get; set; }
        public List<Price> SalePrice { get; set; }
        public List<Price> SalePriceDifference { get; set; }
        public AddCollectPrice AddCollectPrice { get; set; }
        public IEnumerable<RefundPrice> RefundPrice { get; set; }
        public List<PenaltyPrice> PenaltyPrices { get; set; }
        public List<Price> OriginalSalePrice { get; set; }
        public IEnumerable<string> PriceClassRefIds { get; set; }
    }
}
