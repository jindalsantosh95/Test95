namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS
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
            DoNotMail,
            PreferredPartnerFFP
        }
        public AccountPreferenceTypes? PreferenceType { get; set; }
        public string PreferenceValue { get; set; }
    }
}
