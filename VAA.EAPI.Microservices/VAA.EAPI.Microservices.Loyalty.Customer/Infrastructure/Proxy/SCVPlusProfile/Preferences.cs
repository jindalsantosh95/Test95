namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class Preferences
    {
        public enum PreferenceTypeEnum
        {
            VAAMarketingPermissions = 1,
            VAPartnersMarketingPermissions = 2,
            EmailCommunication = 3,
            PostCommunication = 4,
            FaxCommunication = 5,
            FlyingClubNews = 6,
            Language = 7,
            PhoneCommunication = 8,
            DoNotSMS = 9,
            DoNotEmail = 10,
            DoNotPost = 11,
            DoNotCall = 12,
            DoNotFax = 13,
            Communication = 14,
            DoNotMail = 15
        }

        public PreferenceTypeEnum? PreferenceType { get; set; }
        public Preferences(PreferenceTypeEnum? preferenceType = default, string preferenceValue = default)
        {
            PreferenceType = preferenceType;
            PreferenceValue = preferenceValue;
        }

        public string PreferenceValue { get; set; }
    }
}
