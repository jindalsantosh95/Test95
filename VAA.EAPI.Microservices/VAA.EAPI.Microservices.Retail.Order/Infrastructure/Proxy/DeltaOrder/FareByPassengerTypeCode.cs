using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class FareByPassengerTypeCode
    {
        public string FareId { get; set; }
        public FarePrice FarePrice { get; set; }
        public string PassengerTypeCode { get; set; }
        public int PassengerTypeCodeCount { get; set; }
        public string FareCalculationText { get; set; }
        public List<string> TicketDesignatorCode { get; set; }
        public List<FareRulesByFareBreak> FareRulesByFareBreak { get; set; }
        public List<string> FareRulesCode { get; set; }
        public List<Endorsement> Endorsement { get; set; }
        public List<FareSegment> FareSegments { get; set; }
    }
}
