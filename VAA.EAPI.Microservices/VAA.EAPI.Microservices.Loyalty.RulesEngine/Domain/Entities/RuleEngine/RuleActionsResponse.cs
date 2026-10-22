using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class RuleActionsResponse
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string MemberStatus { get; set; }
        public RuleResponse RuleResponse { get; set; }
    }

    public class Action
    {
        public string ActionID { get; set; }
        public string PromotionID { get; set; }
        public string RuleSeqIDFulfilled { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Customer : CustomerInfo
    {
        public string MillionMilerIndicator { get; set; }
        public string LifeTimeGoldIndicator { get; set; }
        public string CenturionFlag { get; set; }
        public string CompanionFlightawardflag { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public string UpgradeRewardflag { get; set; }
        public string ExpiredFlag { get; set; }
        public string LoungePassflag { get; set; }
        public string S2GFlag { get; set; }
        public string S2GFTGFlag { get; set; }
        public int VSFlownMiles { get; set; }
        public int GoldCounter { get; set; }
        public int VALifetimePointsTotal { get; set; }
        public int RetainPointsTotal { get; set; }
        public int VSJCDISectorCount { get; set; }
        public string FastTrackId { get; set; }
        public int VATotalFastTrackTierPts { get; set; }
        public int RollingPointsTotal { get; set; }
        public string InstGoldFirstVSFlight { get; set; }
        public string VAInstantGoldId { get; set; }
        public string InstSilverFirstVSFlight { get; set; }
        public string InstantSilverId { get; set; }
        public string ReferSSAGoldId { get; set; }
        public string TierApprovalStatus { get; set; }
        public string FirstVSFlightActivity { get; set; }
        public RuleResponse RuleResponse { get; set; }
    }

    public class TierRuleEntityState
    {
        public string CorrelationId { get; set; }
        public List<Customer> Customers { get; set; }
    }
}
