using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.MyAccount.Requests.Endpoint
{
    public static class MicrosoftGraph
    {

        public static HttpResponseMessage GetUserDetailsByEmailOrFCNumber(string bearer, string searchParameter)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Content-Type", "application/json" }
            };

            // Escape for OData string literal (') and URL-encode to avoid reserved characters breaking the URI / $filter.
            string escapedSearchParameter = (searchParameter ?? string.Empty).Replace("'", "''");
            escapedSearchParameter = System.Uri.EscapeDataString(escapedSearchParameter);

            string url = Constants.Application.AzureB2CGetMemberDetailsEndpoint.Replace("{searchParameter}", escapedSearchParameter);
            return WebRequest.Get(url, headers);
        }

        public static HttpResponseMessage PasswordReset(string bearer, string id, string password)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Content-Type", "application/json" }
            };

            string url = $"https://graph.microsoft.com/v1.0/users/{id}";

            string json = $@"{{
                                ""passwordProfile"": 
                                {{
                                    ""forceChangePasswordNextSignIn"": false,
                                    ""password"": ""{password}""
                                }}
                             }}";

            return WebRequest.Json.Patch(url, json, headers);
        }
    }
}
