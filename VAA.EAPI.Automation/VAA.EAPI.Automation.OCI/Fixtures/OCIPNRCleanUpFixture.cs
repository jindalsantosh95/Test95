using VAA.EAPI.Automation.OCI.Helpers;

namespace VAA.EAPI.Automation.OCI.Fixtures
{
    public sealed class OCIPNRCleanupFixture
    {
        public OCIPNRCleanupFixture()
        {
            OCIHelper.OCIPNRDeletionFromStorageTableAsync("OCIPNRs").Wait();
        }

        public static async Task<OCIPNRCleanupFixture> CreateAsync()
        {
            var fixture = new OCIPNRCleanupFixture();

            await OCIHelper.OCIPNRDeletionFromStorageTableAsync("OCIPNRs");

            return fixture;
        }
    }
}