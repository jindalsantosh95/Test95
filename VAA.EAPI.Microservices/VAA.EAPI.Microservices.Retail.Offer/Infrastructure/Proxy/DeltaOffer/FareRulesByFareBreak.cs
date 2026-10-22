namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FareRulesByFareBreak
    {
        public bool ChangeableFarebreak { get; set; }
        public string DestinationAirportCode { get; set; }
        public string FareRuleAccountCode { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string OriginAirportCode { get; set; }
        public bool Refundable { get; set; }
        public string TicketDesignatorCode { get; set; }
        public string TicketingLocalTs { get; set; }
        public string TicketingUtcTs { get; set; }
    }
}
