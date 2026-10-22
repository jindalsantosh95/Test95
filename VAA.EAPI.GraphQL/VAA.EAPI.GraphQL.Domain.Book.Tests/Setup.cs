using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests
{
    public static class Setup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"MyAccount:AddressLookUpURLForAddressDetailsV2", "http://tempuri.org"},
                {"MyAccount:IdentityMSURLForEnrolMembertoAzureB2C", "http://tempuri.org"},
                {"MyAccount:AddressLookUpURLForAddressDetails", "false"},
                {"MyAccount:AddressLookup:AppId", "testvalue"},
                {"MyAccount:IdentityMSURLForLoginTokenValidation", "testvalue"},
                {"MyAccount:CustomerMSURLForMemberDetails", "testvalue"},
                {"Enrolment:OcpKey", "testvalue"},
                {"Enrolment:AppId", "testvalue"},
                {"IdentityMS:AppId", "testvalue"},
                {"Identity:OcpSubID", "testvalue"},
                {"Customer:OcpSubID", "testvalue"},
                {"Customer:AppId", "testvalue"},
                {Common.Constants.Environment.CustomerMsUrl, "https://domain/environment/microservices" },
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
