using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Tests
{
    public static class Setup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"CRMMS:EnableResponseUrl", "http://tempuri.org"},
                {"CRMMS:Url", "testvalue"},
                {"CRMMS:OcpSubID", "testvalue"},
                {"CRMMS:AppId", "testvalue"},
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
