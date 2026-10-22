using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.MyAccount.Requests.Endpoint
{
    public static class IdentityFacadeApi
    {
        public static HttpResponseMessage ResetPassword(string fcNo, string password)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                {"Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("MYA-PasswordResetOcpKey")}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/identityfacadeapi-dev/Member/ProfileUpdate"), Factory.IdentityFacadeApi.ResetPassword(fcNo, password), headers);
        }
    }
}
