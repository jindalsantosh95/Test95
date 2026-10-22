using VAA.EAPI.Automation.Core.Helpers.StorageAccount;

namespace VAA.EAPI.Automation.Book.Provisioning.Pnr.Sinks
{
    public sealed class OciMobileAppPnrSink : IPnrSink
    {
        public Task WriteAsync(CreatedPnr pnr)
            => WriteToAzureStorage.AddOciMobileAppPnrsToStorageTable(pnr.Config);
    }
}
