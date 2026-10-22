using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.OCI.Constants;

namespace VAA.EAPI.Automation.OCI.Tests.PnrsCreation
{
    [Trait("Category", Traits.MobileAppOCIPNRCreation)]
    public class MobileAppRevenuePNRCreation : RevenuePNRCreationTest
    {
        public MobileAppRevenuePNRCreation(TokenFixture tokenFixture) : base(tokenFixture) { }

        protected override Task WritePnrToStorage(TestConfig config)
            => WriteToAzureStorage.AddOciMobileAppPnrsToStorageTable(config);
    }
}
