using Microsoft.Azure.Cosmos.Table;
using System.Net;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.TableStorage.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.TableStorage
{
    public class TableStorageInsertService : ITableStorageInsertService
    {
        public async Task<bool> TryInsertAsync<T>(string tableName, T entity, CancellationToken cancellationToken) where T : TableEntity
        {
            var table = await GetTableAsync(tableName);

            try
            {
                await table.ExecuteAsync(TableOperation.Insert(entity), null, null, cancellationToken);
                return true;
            }
            catch (StorageException exception) when (exception.RequestInformation?.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                return false;
            }
        }

        private static async Task<CloudTable> GetTableAsync(string tableName)
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigReader.Get(Constants.Environment.MicroServiceDBKey));
            var table = storageAccount.CreateCloudTableClient(new TableClientConfiguration()).GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }
    }
}
