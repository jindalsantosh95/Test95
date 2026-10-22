using VAA.EAPI.Automation.Core.Helpers.StorageAccount;

namespace VAA.EAPI.Automation.Book.Provisioning.Pnr.Sinks
{
    public sealed class MobileAppPnrSink : IPnrSink
    {
        public Task WriteAsync(CreatedPnr pnr)
            => WriteToAzureStorage.AddPNRsForMobileAppToAzureStorageTable(pnr.Config);
    }
}
