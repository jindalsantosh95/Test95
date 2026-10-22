namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class Endorsement
    {
        public string baseFareCode { get; set; }
        public List<string> bookingCodes { get; set; }
        public Carrier carrier { get; set; }
        public string destinationAirportCode { get; set; }
        public string destinationCity { get; set; }
        public string destinationCityCode { get; set; }
        public string extendedFareCode { get; set; }
        public string fareTariffNum { get; set; }
        public string fbrFareTariffNum { get; set; }
        public string fbrRuleNum { get; set; }
        public string fbrRuleTariffNum { get; set; }
        public string flightSegmentId { get; set; }
        public string flightLegId { get; set; }
        public string tripId { get; set; }
        public string originAirportCode { get; set; }
        public string originCity { get; set; }
        public string originCityCode { get; set; }
        public OriginalSalesAdjustedPrice originalSalesAdjustedPrice { get; set; }
        public string paxType { get; set; }
        public List<string> penaltyIds { get; set; }
        public string rec1FareType { get; set; }
        public string ruleNum { get; set; }
        public string ruleTariffNum { get; set; }
        public SalesAdjustedPrice salesAdjustedPrice { get; set; }
        public string ticketDesignator { get; set; }
        public string tripType { get; set; }
    }
}
