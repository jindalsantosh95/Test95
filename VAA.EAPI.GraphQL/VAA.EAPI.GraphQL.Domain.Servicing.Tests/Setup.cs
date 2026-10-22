using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Tests
{
    public static class Setup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {Common.Constants.Environment.GetBookingUrl, "https://testurl.api.com"},
                {"OrderSearch:AppId", "testvalue"},
                {"OrderSearch:OcpSubID", "testvalue"},
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
