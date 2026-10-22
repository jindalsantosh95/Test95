namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class OfferItemPricing
    {
        public string? FareCalculationText { get; set; }
        public IEnumerable<Price> SalePrice { get; set; }
        public IEnumerable<Price>? SalePriceDifference { get; set; }
        public AddCollectPrice? AddCollectPrice { get; set; }
        public IEnumerable<PenaltyPrice>? PenaltyPrices { get; set; }
        public IEnumerable<Price>? OriginalSalePrice { get; set; }
    }
}