using VAA.EAPI.Automation.Core;


namespace VAA.EAPI.Automation.MobileApp.Requests.Endpoints
{
    public static class CjsAirportEndpoint
    {
        private static readonly string baseUrl = Core.Helpers.ConfigReader.Get("CjsBaseUrl");

        public static HttpResponseMessage AirportData(string searchCriteria)
        {
            return WebRequest.Get($"{baseUrl}{searchCriteria}");
        }
    }
}
