using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class AdditionalOfferProperties
    {
        public bool? LowestFare { get; set; }
        public bool? LowestFareInWeek { get; set; }
        public bool? LowestFareInCalendar { get; set; }
        public bool? UpgradeAvailable { get; set; }
        public bool? SeatSelectable { get; set; }
        public bool? Offered { get; set; }
        public bool? SoldOut { get; set; }
        public bool? AvailableAtPostPurchase { get; set; }
        public bool? MultipleClassOfService { get; set; }
        public bool? MultipleProducts { get; set; }
        public bool? MultipleCabins { get; set; }
        public bool? PayWithMilesEligible { get; set; }
        public bool? Upsell { get; set; }
        public bool? Selected { get; set; }
        public bool? MixAndMatch { get; set; }
        public bool? Refundable { get; set; }
        public bool? UnavailableForSale { get; set; }
        public bool? DiscountAvailable { get; set; }
        public bool? WebPriceAvailable { get; set; }
        public bool? Changeable { get; set; }
        public bool? LowestFareByGroup { get; set; }
        public bool? LowestFareByBrand { get; set; }
        public int AvailableSeatCnt { get; set; }
        public bool? DisplaySeatCntWarning { get; set; }
        public string DominantSegmentBrandId { get; set; }
        public string DominantSegmentCabin { get; set; }
        public string DominantSegmentCabinPadisCode { get; set; }
        public bool? MixedBrandedProductExperience { get; set; }
        public bool? FareChanged { get; set; }
        public bool? NewTicket { get; set; }
        public bool? PartialUpsell { get; set; }
        public string DayOfWeekText { get; set; }
        public string DayOfMonthText { get; set; }
        public string MonthText { get; set; }
        public int TotalTripStopCnt { get; set; }
        public int ItineraryDurationDayCnt { get; set; }
        public string FlexDestinationAirportCode { get; set; }
        public int PriorityNum { get; set; }
        public string FlexDepartDate { get; set; }
        public string FlexReturnDate { get; set; }
        public bool? FareAvailable { get; set; }
        public PolicyStatus TravelPolicyStatus { get; set; }
        public bool? Cancelable { get; set; }
        public IEnumerable<Product> OfferBadges { get; set; }
        public IEnumerable<string> SecondarySolutionRefIds { get; set; }
        public string FareType { get; set; }
        public IEnumerable<DiscountInEligibilityInfo> DiscountInEligibilityList { get; set; }
    }
}
