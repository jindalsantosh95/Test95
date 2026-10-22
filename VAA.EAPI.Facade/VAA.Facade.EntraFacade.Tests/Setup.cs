using VAA.EAPI.Common.Helpers;

namespace VAA.Facade.EntraFacade.Tests
{
    public static class Setup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"IdentityMS:Url", "http://tempuri.org" },
                {"IdentityMS:AppId", "testappid" },
                {"IdentityMS:OcpKey", "testocpkey" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
