using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class FlyingClubMemberDocument
    {
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public string CustomerId { get; set; }
        public string TierCode { get; set; }
        public DateTime? GoldStartDate { get; set; }
        public string DowngradeProhibitFlag { get; set; }
        public List<MemberAttributeDocument> MemberAttributes { get; set; }
        public MemberShipCardDetails MemberShipCardDetails { get; set; }

    }
}
