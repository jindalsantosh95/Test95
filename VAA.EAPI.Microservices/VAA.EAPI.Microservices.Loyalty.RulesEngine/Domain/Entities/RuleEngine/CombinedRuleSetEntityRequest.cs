using System;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class CombinedRuleSetEntityRequest
    {
        public string CustomerId { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string VAReasonCode { get; set; }
        public string VABookingType { get; set; }
        public string VAReward1Code { get; set; }
        public string VAPDMFileType { get; set; }
        public string Miles { get; set; }
        public string TransactionType { get; set; }
        public string MarketingCode { get; set; }
        public string OriginCountry { get; set; }
        public string DestinationCountry { get; set; }
        public string BookingClass { get; set; }
        public string PartnerCode { get; set; }
        public string ProductName { get; set; }
        public string ProductId { get; set; }
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public DateTime? TicketIssueDate { get; set; }
        public DateTime? ActivityDate { get; set; }
        public string FirstVSFlightActivity { get; set; }
        public string ManualCovidBonusCount { get; set; }
        public string EarnActivity { get; set; }
        public string MemberNumber { get; set; }
        public string MemberType { get; set; }
        public string MemberStatus { get; set; }
        public string MemberDescription { get; set; }
        public string CurrentStatus { get; set; }
        public string CurrentTier { get; set; }
        public string FltCount_CorpBonus { get; set; }
        public string FltCount_Generic { get; set; }
        public string VAInstantSilverStartDate { get; set; }
        public string VAInstantSilverEndDate { get; set; }
        public string InstantSilverId { get; set; }
        public string InstantGoldId { get; set; }
        public string VAInstantGoldStartDate { get; set; }
        public string VAInstantGoldEndDate { get; set; }
        public string TierCount { get; set; }
    }
}
