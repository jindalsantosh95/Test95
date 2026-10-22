using System;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Base;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities
{
    public class PreferenceAudit : LoyaltyAudit
    {
        public string SourceIdentifier { get; set; }
        public string VAMarketingPreference { get; set; }
        public string VAPartnersMarketingPermissions { get; set; }
        public string DoNotCall { get; set; }
        public string DoNotEmail { get; set; }
        public string DoNotSMS { get; set; }
        public string PreferredPartnerFFP { get; set; }
        public string Language { get; set; }
        public string EmailCommunication { get; set; }
        public string PostCommunication { get; set; }
        public string FaxCommunication { get; set; }
        public string FlyingClubNews { get; set; }
        public string PhoneCommunication { get; set; }
        public string DoNotPost { get; set; }
        public string DoNotFax { get; set; }
        public string Communication { get; set; }
        public string DoNotMail { get; set; }
        public string PreferenceUpdatedBy { get; set; }
        public DateTime? PreferenceUpdatedOn { get; set; }
    }
}