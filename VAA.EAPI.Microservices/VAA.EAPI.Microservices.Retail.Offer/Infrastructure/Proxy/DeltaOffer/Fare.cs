using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Fare
    {
        public int? AvailableSeatCnt { get; set; }
        public BaggageAllowance BaggageAllowance { get; set; }
        public IEnumerable<BrandByFlightLeg> BrandByFlightLegs { get; set; }
        public IEnumerable<Product> FareProduct { get; set; }
        public bool ChangeableFare { get; set; }
        public bool DiscountAvailable { get; set; }
        public string DominantSegmentBrandId { get; set; }
        public City EbusinessPointOfSaleCity { get; set; }
        public IEnumerable<FareEndorsement> Endorsement { get; set; }
        public string FareCalculationText { get; set; }
        public bool FareChanged { get; set; }
        public IEnumerable<FareRestriction> FareRestrictionByFlightSegment { get; set; }
        public IEnumerable<FareRulesByFareBreak> FareRulesByFareBreak { get; set; }
        public IEnumerable<string> FareRulesBySegmentNum { get; set; }
        public IEnumerable<string> FareRulesCode { get; set; }
        public ContentUrl FareRuleUrl { get; set; }
        public string GroupId { get; set; }
        public string ItaFareId { get; set; }
        public string LongestHaulClassOfServiceCode { get; set; }
        public bool LowestFare { get; set; }
        public string LowestFareByGroupId { get; set; }
        public bool MixAndMatch { get; set; }
        public bool MultipleCabins { get; set; }
        public bool MultipleCos { get; set; }
        public bool MultipleProducts { get; set; }
        public bool NewTicket { get; set; }
        public bool OfferAllowed { get; set; }
        public bool PartialUpsell { get; set; }
        public Passenger Passenger { get; set; }
        public bool PayWithMilesEligible { get; set; }
        public City PointOfSaleCity { get; set; }
        public string ReedemerPassengerTypeCode { get; set; }
        public bool Refundable { get; set; }
        public RepriceQuote RepriceQuote { get; set; }
        public bool SeatSelectable { get; set; }
        public bool Selected { get; set; }
        public ContentUrl SelectUrl { get; set; }
        public bool SoldOut { get; set; }
        public string SolutionId { get; set; }
        public IEnumerable<string> SpecialFareRulesCode { get; set; }
        public IEnumerable<TravelRelatedServiceTax> Tax { get; set; }
        public IEnumerable<string> TicketDesignatorCode { get; set; }
        public string TravelerPassengerTypeCode { get; set; }
        public bool UpgradeAvailable { get; set; }
        public bool Upsell { get; set; }
        public bool WebPriceAvailable { get; set; }
        public bool GlobalUpgradeCertificateStatus { get; set; }
        public bool RegionalUpgradeCertificateStatus { get; set; }
        public IEnumerable<FarePrice> FarePrice { get; set; }
        public PriceCalendar PriceCalendar { get; set; }
        public IEnumerable<DiscountInEligibilityInfo> DiscountInEligibilityList { get; set; }
    }
}
