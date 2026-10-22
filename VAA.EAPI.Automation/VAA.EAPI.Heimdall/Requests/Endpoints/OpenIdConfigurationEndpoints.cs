using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.Heimdall.Requests.Endpoints
{
    public static class OpenIdConfigurationEndpoints
    {
        public static HttpResponseMessage OpenIdConfiguration(string endpoint)
        {
            return WebRequest.Get(ConfigReader.Get(endpoint));
        }
    }
}
