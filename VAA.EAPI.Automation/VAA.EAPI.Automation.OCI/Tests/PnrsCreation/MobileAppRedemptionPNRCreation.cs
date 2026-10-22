using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.OCI.Constants;

namespace VAA.EAPI.Automation.OCI.Tests.PnrsCreation
{
    [Collection("RedemptionPNRCreation")]
    [Trait("Category", Traits.MobileAppOCIPNRCreation)]
    public class MobileAppRedemptionPNRCreation : RedemptionPNRCreationTest
    {
        public MobileAppRedemptionPNRCreation(TokenFixture tokenFixture) : base(tokenFixture) { }

        protected override Task WritePnrToStorage(TestConfig config)
            => WriteToAzureStorage.AddOciMobileAppPnrsToStorageTable(config);
    }
}
