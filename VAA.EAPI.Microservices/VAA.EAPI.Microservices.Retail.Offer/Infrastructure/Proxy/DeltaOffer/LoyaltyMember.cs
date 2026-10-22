using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class LoyaltyMember
    {
        public string LoyaltyMemberId { get; set; }
        public string LoyaltyProgramTierCode { get; set; }
        public string LoyaltyMemberOwnerCode { get; set; }
        public IEnumerable<LoyaltyMemberCorporateAgreement> LoyaltyMemberCorporateAgreement { get; set; }
        public bool UpgradeEligible { get; set; }
        public bool PayWithMilesEligible { get; set; }
        public IEnumerable<string> AffiliationCodes { get; set; }
        public string SkipAffiliation { get; set; }
        public IEnumerable<LoyaltyCertificate> Certificates { get; set; }
        public string IneligibleReasonCode { get; set; }
    }
}
