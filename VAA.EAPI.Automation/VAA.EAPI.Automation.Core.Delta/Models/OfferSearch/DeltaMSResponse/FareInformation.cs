namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class FareInformation
    {
        public int availableSeatCnt { get; set; }
        public List<BrandByFlightLeg> brandByFlightLegs { get; set; }
        public List<FareProduct> fareProduct { get; set; }
        public string dominantSegmentBrandId { get; set; }
        public List<Endorsement> endorsement { get; set; }
        public string fareCalculationText { get; set; }
        public List<object> fareRestrictionByFlightSegment { get; set; }
        public List<object> fareRulesByFareBreak { get; set; }
        public List<string> fareRulesCode { get; set; }
        public FareRuleUrl fareRuleUrl { get; set; }
        public string groupId { get; set; }
        public string itaFareId { get; set; }
        public Passenger passenger { get; set; }
        public PointOfSaleCity pointOfSaleCity { get; set; }
        public string reedemerPassengerTypeCode { get; set; }
        public string solutionId { get; set; }
        public List<object> specialFareRulesCode { get; set; }
        public List<object> tax { get; set; }
        public List<string> ticketDesignatorCode { get; set; }
        public string travelerPassengerTypeCode { get; set; }
        public List<FarePrice> farePrice { get; set; }
        public List<object> discountInEligibilityList { get; set; }
    }
}
