using VAA.EAPI.Automation.Core.Helpers.StorageAccount;

namespace VAA.EAPI.Automation.Book.Provisioning.Pnr.Sinks
{
    public sealed class SeatScenariosPnrSink : IPnrSink
    {
        public Task WriteAsync(CreatedPnr pnr)
            => WriteToAzureStorage.AddPNRsForSeatScenariosToAzureStorageTable(pnr.Config);
    }
}
