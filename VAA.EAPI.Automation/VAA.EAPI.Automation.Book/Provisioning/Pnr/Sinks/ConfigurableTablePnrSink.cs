using VAA.EAPI.Automation.Core.Helpers.StorageAccount;

namespace VAA.EAPI.Automation.Book.Provisioning.Pnr.Sinks
{
    public enum PnrTableSchema
    {
        MobileApp,
        LoyaltyAccount,
        Oci,
        OciMobileApp,
        SeatScenario,
    }

    public sealed class ConfigurableTablePnrSink : IPnrSink
    {
        private readonly PnrTableSchema schema;
        private readonly string tableName;

        public ConfigurableTablePnrSink(PnrTableSchema schema, string tableName)
        {
            this.schema = schema;
            this.tableName = tableName;
        }

        public Task WriteAsync(CreatedPnr pnr)
        {
            return schema switch
            {
                PnrTableSchema.MobileApp => WriteToAzureStorage.AddPNRsForMobileAppToAzureStorageTable(pnr.Config, tableName),
                PnrTableSchema.LoyaltyAccount => WriteToAzureStorage.AddLoyaltyAccountPNRsForMobileAppToAzureStorageTable(pnr.Config, tableName),
                PnrTableSchema.Oci => WriteToAzureStorage.AddOCIPNRToAzureStorageTable(pnr.Config, tableName),
                PnrTableSchema.OciMobileApp => WriteToAzureStorage.AddOciMobileAppPnrsToStorageTable(pnr.Config, tableName),
                PnrTableSchema.SeatScenario => WriteToAzureStorage.AddPNRsForSeatScenariosToAzureStorageTable(pnr.Config, tableName),
                _ => throw new ArgumentOutOfRangeException(nameof(schema), schema, "Unsupported PNR table schema."),
            };
        }
    }
}
