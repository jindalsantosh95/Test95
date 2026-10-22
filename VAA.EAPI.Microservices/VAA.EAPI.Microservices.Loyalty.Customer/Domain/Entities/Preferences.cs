using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
{
    public class Preferences
    {
        public enum PreferenceTypes
        {
            [EnumMember(Value = "VAAMarketingPermissions")]
            VAAMarketingPermissions = 0,
            [EnumMember(Value = "VAPartnersMarketingPermissions")]
            VAPartnersMarketingPermissions = 1,
            [EnumMember(Value = "VaGroupMarketingPermissions")]
            VaGroupMarketingPermissions = 2,
            [EnumMember(Value = "PostCommunication")]
            PostCommunication = 3,
            [EnumMember(Value = "FaxCommunication")]
            FaxCommunication = 4,
            [EnumMember(Value = "FlyingClubNews")]
            FlyingClubNews = 5,
            [EnumMember(Value = "Language")]
            Language = 6,
            [EnumMember(Value = "PhoneCommunication")]
            PhoneCommunication = 7,
            [EnumMember(Value = "DoNotSMS")]
            DoNotSMS = 8,
            [EnumMember(Value = "DoNotEmail")]
            DoNotEmail = 9,
            [EnumMember(Value = "DoNotPost")]
            DoNotPost = 10,
            [EnumMember(Value = "DoNotCall")]
            DoNotCall = 11,
            [EnumMember(Value = "DoNotFax")]
            DoNotFax = 12,
            [EnumMember(Value = "Communication")]
            Communication = 13,
            [EnumMember(Value = "DoNotMail")]
            DoNotMail = 14,
            [EnumMember(Value = "EmailCommunication")]
            EmailCommunication = 15,
            [EnumMember(Value = "PreferredPartnerFFP")]
            PreferredPartnerFFP = 16,
        }

        public PreferenceTypes? PreferenceType { get; set; }
        public string PreferenceValue { get; set; }
    }
}
