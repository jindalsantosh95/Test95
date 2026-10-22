namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer
{
    public class AccountPreference
    {
        public enum AccountPreferenceTypes
        {
            VaaMarketingPermissions,
            VaPartnersMarketingPermissions,
            VaGroupMarketingPermissions,
            EmailCommunication,
            PostCommunication,
            FaxCommunication,
            FlyingClubNews,
            Language,
            PhoneCommunication,
            DoNotSms,
            DoNotEmail,
            DoNotPost,
            DoNotCall,
            DoNotFax,
            Communication,
            DoNotMail
        }
        public AccountPreferenceTypes? PreferenceType { get; set; }
        public string PreferenceValue { get; set; }
    }
}
