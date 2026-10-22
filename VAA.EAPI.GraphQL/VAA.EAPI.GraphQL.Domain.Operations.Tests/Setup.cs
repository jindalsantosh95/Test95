using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Domain.Operations.Tests
{
    public static class Setup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {Operations.Constants.Environment.OperationsPassengerManifestUrl, "https://testurl.api.com"},
                {"Operations:PassengerManifestAcceptHeader", "testvalue"},
                {"Operations:PassengerManifestOcpKey", "testvalue"},
                {"Operations:PassengerManifestAppId", "testvalue"},
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
