using VAA.EAPI.Automation.OCI.Constants;
using VAA.EAPI.Automation.OCI.Helpers;

namespace VAA.EAPI.Automation.OCI.Data
{
    [Collection("Sequential")]

    public class OCIPNRDeletion
    {
        [Fact]
        [Trait("Category", Traits.OCIPNRDeletion)]
        public async Task OCIPNRDeletionFromStorageTable()
        {
            await OCIHelper.OCIPNRDeletionFromStorageTableAsync("OCIPNRs");
        }

        [Fact]
        [Trait("Category", Traits.OCIPNRDeletion)]
        public async Task MobileAppOciPnrDeletionFromStorageTable()
        {
            await OCIHelper.OCIPNRDeletionFromStorageTableAsync("OnlineCheckInMobileAppData");
        }
    }
}