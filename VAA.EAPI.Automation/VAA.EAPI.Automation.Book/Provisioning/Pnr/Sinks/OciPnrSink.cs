using VAA.EAPI.Automation.Core.Helpers.StorageAccount;

namespace VAA.EAPI.Automation.Book.Provisioning.Pnr.Sinks
{
    public sealed class OciPnrSink : IPnrSink
    {
        public Task WriteAsync(CreatedPnr pnr)
            => WriteToAzureStorage.AddOCIPNRToAzureStorageTable(pnr.Config);
    }
}
