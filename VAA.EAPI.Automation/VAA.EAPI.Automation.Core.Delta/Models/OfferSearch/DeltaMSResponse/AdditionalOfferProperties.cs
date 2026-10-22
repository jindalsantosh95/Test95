namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class AdditionalOfferProperties
    {
        public bool lowestFare { get; set; }
        public bool upgradeAvailable { get; set; }
        public bool seatSelectable { get; set; }
        public bool offered { get; set; }
        public bool soldOut { get; set; }
        public bool availableAtPostPurchase { get; set; }
        public bool multipleClassOfService { get; set; }
        public bool multipleProducts { get; set; }
        public bool multipleCabins { get; set; }
        public bool payWithMilesEligible { get; set; }
        public bool upsell { get; set; }
        public bool selected { get; set; }
        public bool mixAndMatch { get; set; }
        public bool refundable { get; set; }
        public bool unavailableForSale { get; set; }
        public bool discountAvailable { get; set; }
        public bool webPriceAvailable { get; set; }
        public bool changeable { get; set; }
        public bool lowestFareByGroup { get; set; }
        public bool lowestFareByBrand { get; set; }
        public int availableSeatCnt { get; set; }
        public string dominantSegmentBrandId { get; set; }
        public string dominantSegmentCabin { get; set; }
        public string dominantSegmentCabinPadisCode { get; set; }
        public bool mixedBrandedProductExperience { get; set; }
        public int priorityNum { get; set; }
        public List<OfferBadge> offerBadges { get; set; }
        public List<string> secondarySolutionRefIds { get; set; }
        public string fareType { get; set; }
        public List<object> discountInEligibilityList { get; set; }
    }
}
