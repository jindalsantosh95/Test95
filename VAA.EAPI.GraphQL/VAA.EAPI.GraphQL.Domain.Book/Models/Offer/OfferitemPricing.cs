using VAA.EAPI.GraphQL.Domain.Book.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Offer
{
    public class OfferitemPricing
    {
        public string FareCalculationText { get; set; }
        public IEnumerable<Price> SalePrice { get; set; }
        public IEnumerable<Price> SalePriceDifference { get; set; }
        public AddCollectPrice AddCollectPrice { get; set; }
        public IEnumerable<PenaltyPrice> PenaltyPrices { get; set; }
        public IEnumerable<Price> OriginalSalePrice { get; set; }
    }
}
