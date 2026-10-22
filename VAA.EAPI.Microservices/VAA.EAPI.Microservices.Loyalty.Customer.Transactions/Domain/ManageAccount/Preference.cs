using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount
{
    public class Preference
    {
        public PreferenceTypes? PreferenceType { get; set; }
        public string PreferenceValue { get; set; }
    }
    public enum PreferenceTypes
    {
        VAAMarketingPermissions,
        VAPartnersMarketingPermissions,
        EmailCommunication,
        PostCommunication,
        FaxCommunication,
        FlyingClubNews,
        Language,
        PhoneCommunication,
        DoNotSMS,
        DoNotEmail,
        DoNotPost,
        DoNotCall,
        DoNotFax,
        Communication,
        DoNotMail,
        PreferredPartnerFFP
    }
}
