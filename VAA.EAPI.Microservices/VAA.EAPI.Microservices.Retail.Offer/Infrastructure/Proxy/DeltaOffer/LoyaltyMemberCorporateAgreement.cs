using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class LoyaltyMemberCorporateAgreement
    {
        public string AgreementId { get; set; }
        public string AgreementUserId { get; set; }
        public string AgreementTypeCode { get; set; }
        public string AgreementTypeDesc { get; set; }
        public IEnumerable<string> AgreementPermissionTypeCodes { get; set; }
        public string AgreementTypeCodeId { get; set; }
        public string AgreementEffectiveDate { get; set; }
        public string AgreementDiscontinueDate { get; set; }
        public bool ApplyCorporateTravelPolicies { get; set; }
        public bool CanBookBasicEconomy { get; set; }
    }
}
