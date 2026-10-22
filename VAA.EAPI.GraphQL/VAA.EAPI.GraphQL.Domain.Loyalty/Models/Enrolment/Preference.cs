namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment
{
    public class Preference
    {
        public enum PreferenceTypes
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
        public PreferenceTypes? PreferenceType { get; set; }
        public string? PreferenceValue { get; set; }
    }
}
