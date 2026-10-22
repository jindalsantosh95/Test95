using Azure;
using Azure.Data.Tables;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Infrastructure.AzureStorage;
using LegacyTableEntity = Microsoft.Azure.Cosmos.Table.TableEntity;

namespace VAA.EAPI.Automation.Core.Helpers.StorageAccount
{
    public static class StorageAccountHelper
    {
        private static readonly TimeSpan PnrRetentionPeriod = TimeSpan.FromDays(30);
        private const int MaxPnrRowsPerPage = 500;
        private const int MaxBatchOperations = 100;
        private const string NdcPnrTableName = "NDCPNRsTable";
        private static readonly Lazy<TableClient> NdcPnrTableClient = new(CreateNdcPnrTableClient);

        private static readonly Lazy<string> StorageConnectionString = new(() => Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString"));

        public static string[] RetrievePNRFromStorageTable(string tableName)
        {
            if (tableName.Is(NdcPnrTableName))
            {
                TableClient tableClient = GetNdcPnrTableClient();
                string partitionKey = Config.Environment.ToLowerInvariant();
                string filter = $"PartitionKey eq '{partitionKey}'";
                Pageable<NdcPnrTableEntity> queryEntities = tableClient.Query<NdcPnrTableEntity>(filter: filter, maxPerPage: MaxPnrRowsPerPage);

                return queryEntities
                    .Select(entity => entity.RowKey)
                    .Where(rowKey => !rowKey.IsNullOrWhiteSpace())
                    .ToArray();
            }

            TableStorageService<LegacyTableEntity> tableStorageService = new TableStorageService<LegacyTableEntity>
            {
                TableName = tableName,
                ConnectionString = StorageConnectionString.Value
            };

            IEnumerable<LegacyTableEntity> entities = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult();

            return entities.Select(entity => entity.PartitionKey).ToArray();
        }

        public static void SavePNRToStorageTable(string pnr, string environment, string bookingType = "Unknown", string paymentType = "Unknown", string seatType = "Unknown")
        {
            if (pnr.IsNullOrWhiteSpace())
                throw new ArgumentException("A PNR is required before saving it to storage.", nameof(pnr));

            if (environment.IsNullOrWhiteSpace())
                throw new ArgumentException("An environment is required before saving a PNR to storage.", nameof(environment));

            TableClient tableClient = GetNdcPnrTableClient();
            string partitionKey = environment.ToLowerInvariant();
            string createdUtc = DateTimeOffset.UtcNow.ToString("O");
            NullableResponse<NdcPnrTableEntity> existingEntity = tableClient.GetEntityIfExists<NdcPnrTableEntity>(partitionKey, pnr);

            if (existingEntity.HasValue && !existingEntity.Value.CreatedUtc.IsNullOrWhiteSpace())
                createdUtc = existingEntity.Value.CreatedUtc;

            NdcPnrTableEntity entity = CreateNdcPnrEntity(pnr, environment, bookingType, paymentType, seatType, createdUtc);

            tableClient.UpsertEntity(entity, TableUpdateMode.Replace);
        }

        public static void CleanupExpiredPNRs(string environment = null)
        {
            try
            {
                TableClient tableClient = GetNdcPnrTableClient();
                DateTimeOffset cutoff = DateTimeOffset.UtcNow.Subtract(PnrRetentionPeriod);
                string cutoffValue = cutoff.ToString("O");
                string filter = environment.IsNullOrWhiteSpace()
                    ? $"CreatedUtc lt '{cutoffValue}'"
                    : $"PartitionKey eq '{environment.ToLowerInvariant()}' and CreatedUtc lt '{cutoffValue}'";

                Pageable<NdcPnrTableEntity> expiredEntities = tableClient.Query<NdcPnrTableEntity>(filter: filter, maxPerPage: MaxPnrRowsPerPage);
                foreach (IEnumerable<TableTransactionAction> batch in CreateCleanupBatches(expiredEntities))
                {
                    try
                    {
                        tableClient.SubmitTransaction(batch);
                    }
                    catch (RequestFailedException storageException) when (storageException.Status == 404)
                    {
                        Console.WriteLine("NDC PNR cleanup skipped an entity already removed by another process.");
                    }
                }
            }
            catch (RequestFailedException storageException) when (storageException.Status == 404)
            {
                Console.WriteLine("NDC PNR cleanup found no table or rows to delete.");
            }
            catch (Exception storageException)
            {
                Console.WriteLine($"NDC PNR cleanup was skipped: {storageException.GetType().Name}: {storageException.Message}");
            }
        }

        public static string RetrieveNDCTestCardsFromStorageTable(string scenario)
        {
            TableStorageService<LegacyTableEntity> tableStorageService = new TableStorageService<LegacyTableEntity>
            {
                TableName = "TestCardsNDC",
                ConnectionString = StorageConnectionString.Value
            };

            LegacyTableEntity pnrTable = tableStorageService.RetrieveItemsAsync(default).GetAwaiter().GetResult().FirstOrDefault(entity => entity.PartitionKey == scenario);

            return pnrTable?.RowKey;
        }

        internal static NdcPnrTableEntity CreateNdcPnrEntity(string pnr, string environment, string bookingType, string paymentType, string seatType, string createdUtc) => new()
        {
            PartitionKey = environment.ToLowerInvariant(),
            RowKey = pnr,
            Environment = environment,
            BookingType = bookingType,
            PaymentType = paymentType,
            SeatType = seatType,
            CreatedUtc = createdUtc
        };

        internal static IEnumerable<IEnumerable<TableTransactionAction>> CreateCleanupBatches(IEnumerable<NdcPnrTableEntity> expiredEntities) => expiredEntities
            .GroupBy(entity => entity.PartitionKey)
            .SelectMany(partitionEntities => partitionEntities
                .Select(entity => new TableTransactionAction(TableTransactionActionType.Delete, entity))
                .Chunk(MaxBatchOperations));

        private static TableClient GetNdcPnrTableClient() => NdcPnrTableClient.Value;

        private static TableClient CreateNdcPnrTableClient()
        {
            TableClient tableClient = new TableClient(StorageConnectionString.Value, NdcPnrTableName);
            tableClient.CreateIfNotExists();
            return tableClient;
        }
    }
}
