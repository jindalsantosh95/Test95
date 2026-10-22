using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [CollectionDefinition("EnvironmentDependentTests", DisableParallelization = true)]
    public class EnvironmentDependentTestsCollection
    {
    }
    public static class Setup
    {
        public const string TestIdentityActivationTokenIssuer = "test-issuer";
        public const string TestIdentitySigningKey = "TestkeySRECDSADSADSAREWEF76545$==+";

        public static void CreateTestEnvironmentSettings()
        {

            Dictionary<string, string> settings = new()
            {
                {"MyAccount:AddressLookUpURLForAddressDetailsV2", "http://tempuri.org"},
                {"Customer:MsUrl","https://Testresources.virginatlantic.com/dev/microservices" },
                {"MyAccount:AddressLookUpURLForAddressDetails", "false"},
                {"MyAccount:EnableMockResponseForFlightDetails", "false"},
                {"AddressLookup:AppId", "testvalue"},
                {"MyAccount:CustomerMSURLForMemberDetails", "testvalue"},
                  {"EnrolmentTokenFlag","true" },
                  {"MyAccount:AllowMemberToEnrolToAzureB2C", "true"},
                  {"MyAccount:MobileEnrolment:MinimumIosVersion", "1.10.0"},
                  {"MyAccount:MobileEnrolment:MinimumAndroidVersion", "1.10.0"},
                  {"Identity:MsUrl","https://Testresources.virginatlantic.com/dev/identitymicroservice" },
                {VAA.EAPI.GraphQL.Common.Constants.Environment.IdentityActivationTokenIssuer, TestIdentityActivationTokenIssuer },
                {VAA.EAPI.GraphQL.Common.Constants.Environment.IdentitySignKey, TestIdentitySigningKey },
                {"myaccount:dedupfcgenurl", "https://Testresources.virginatlantic.com/dev/microservices/GenerateFcNumber" },
                {"vredmarketingfeatureflag","N" },
                {"Enrolment:OcpKey", "testvalue"},
                {"Enrolment:AppId", "testvalue"},
                {"IdentityMS:AppId", "21b92f65-dbe2-4d5f-8072-ccfbe8212f53"},
                {"Identity:OcpSubID", "53dfe11df53145cebd611c1f15cad65d"},
                {"Customer:OcpSubID", "53dfe11df53145cebd611c1f15cad65d"},
                {"Customer:AppId", "53b875d8-61c2-4a3c-90fc-88fd4ef16d44"},
                {"CustTrans:OcpSubID", "testvalue"},
                {"CustTrans:AppId","testvalue" },
                {"CustTrans:MsUrl","https://testresources.virginatlantic.com/dev/customertransactions/"},
                {"VRed:OcpSubID","testvalue" },
                {"Book:VRed:VirginPoints", "testvalue"},
                {"OrderSearch:OcpSubId","testvalue" },
                 {"OrderSearch:AppId","testvalue" },
                {"GetBooking:Url","testvalue" },
                {"myaccount:futurebookingurl", "testvalue"},
                {"MyAccount:EnableMockResponseForActivity", "false"},
                {"myaccount:vredactivityurl", "testurl"},
                {"vred:appid","testvalue" },
                {"MyAccount:EnableMockResponseForManageAccount","false" },
                {"MyAccount:EnableMockResponseForPointDotCom","false" },
                {"myaccount:securitymsurlforgettokenforshopsaway","testurl" },
                {"security:ocpsubid","testId" },
                {"security:appid","testAppId" },
                {"FEAPI:CreateCreditManualAdjustment", "http://tempuri.org" },
                {"FEAPI:CreateManualRedempton", "http://tempuri.org" },
                {"FEAPI:AppId", "testvalue" },
                {"FEAPI:OcpKey", "testvalue" },
                {"MyAccount:IdentityMSURLForVerifyMemberInAzureB2C", "TestIdentityMSUrl" },
                {"MyAccount:CustomerMSURLForEmailUpdate", "testCustomerMSUrl" },
                {"IdentityMS:OcpKey", "53dfe11df53145cebd611c1f15cad65d" },
                {"MyAccount:CustomerMSUrlForUpdateMemberAddress", "testCustomerMSURLforAddressUpdate" },
                {"MyAccount:PostCodeMandatoryCountryList", "GB|US" },
                {"CustTransactionMSURL", "testCustTransactionURL" },
                {"FEAPI:CancelTransaction", "http://tempuri.org" },
                {"commoneapiocpapimkey", "testvalue" },
                {"Book:VRed:AppId", "testvredappid" },
                {"MyAccount:VRedMSURLForCreateTransaction", "http://tempuri.org"},
                {"RefDataMS:GenericURL", "https://Testresources.virginatlantic.com/dev/ReferenceMicroservice/{0}" },
                {"identityms:validatepasswordurl", "https://Testresources.virginatlantic.com/dev/identitymicroservice/ValidatePasswordPolicies" },
                {"Customer:UrlGetbyCustID", "https://Testresources.virginatlantic.com/dev/microservices/customer/{customerid}" },
                {"Customer:UrlGetByMemNum", "https://Testresources.virginatlantic.com/dev/microservices/customer/loyalty/{memberid}" },
                {"skyms:genericurl","https://Testresources.virginatlantic.com/dev/SkyMs/" },
                {"refdatams:ocpsubid", "test" },
                {"skyms:appid","test" },
                { "InRuleMSUrl","https://Testresources.virginatlantic.com/dev/inrule"},
                { "InRule:Url","https://Testresources.virginatlantic.com/dev/inrule"},
                { "InRule:OcpSubID", "testkey" },
                { "InRuleMSAppID", "testappid"},
                { "InRule:AppId", "testappid"},
                { "MemberManagement:MsUrl", "https://Testresources.virginatlantic.com/dev/loyaltymembermanagement" },
                { "MemberManagement:AppId","TestappId" },
                { "MemberManagement:OcpSubID", "TestOcp" },
                { "VRed:MsUrl", "https://Testresources.virginatlantic.com/dev/vred" },
                { "CampaignManagement:CampaignManagementMsUrl", "https://Testresources.virginatlantic.com/sit/campaignmanagement/" },
                { "CampaignManagement:AppId", "testkey" },
                { "CampaignManagement:OcpSubID", "testOcp" },
                { "eapigraph:appid","testvalue" },
                { "eapigraph:clientsecret", "testsecret" },
                { "reference:MsUrl", "https://Testresources.virginatlantic.com/dev/ReferenceMicroservice/" },
                { "ReferenceMSOcpKey", "testKey" },

    };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
