using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.AzureStorage
{
    public class TableStorageService<T> : ITableStorageService<T> where T : TableEntity, ITableEntity, new()
    {
        public string TableName { get; set; }
        public string ConnectionString { get; set; }

        public TableStorageService() { }
        public TableStorageService(string connectionString, string tableName)
        {
            ConnectionString = connectionString;
            TableName = tableName;
        }

        public async Task<T> RetrieveAsync(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await ExecuteTableOperation(retrieveOperation);
            return (T)result;
        }
        public async Task<List<T>> RetrieveItemsAsync(TableContinuationToken continutaionToken)
        {
            List<T> entities = new List<T>();
            do
            {
                var queryResult = (await GetCloudTable())?.ExecuteQuerySegmented(new TableQuery<T>(), continutaionToken);
                if (queryResult != null && queryResult.Results.Count > 0)
                {
                    entities.AddRange(queryResult.Results);
                }
                continutaionToken = queryResult.ContinuationToken;
            } while (continutaionToken != null);
            return entities;
        }

        public async Task<T> InsertOrMergeAsync(T entity)
        {
            var insertOrMergeOperation = TableOperation.InsertOrMerge(entity);
            var result = await ExecuteTableOperation(insertOrMergeOperation);
            return (T)result;
        }

        public async Task<T> DeleteAsync(T entity)
        {
            entity.ETag = "*";
            var deleteOperation = TableOperation.Delete(entity);
            var result = await ExecuteTableOperation(deleteOperation);
            return (T)result;
        }

        private async Task<object> ExecuteTableOperation(TableOperation tableOperation)
        {
            var table = await GetCloudTable();
            var tableResult = await table.ExecuteAsync(tableOperation);
            return tableResult.Result;
        }

        private async Task<CloudTable> GetCloudTable()
        {
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            var table = tableClient.GetTableReference(TableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        public async Task<List<T>> RetrieveItemsInPartitionAsync(string partitionKey, TableContinuationToken continutaionToken)
        {
            List<T> entities = new List<T>();

            var query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);

            do
            {
                var queryResult = (await GetCloudTable())?.ExecuteQuerySegmented(new TableQuery<T>().Where(query), continutaionToken);
                if (queryResult != null && queryResult.Results.Count > 0)
                {
                    entities.AddRange(queryResult.Results);
                }
                continutaionToken = queryResult.ContinuationToken;
            } while (continutaionToken != null);

            return entities;
        }

        public async Task<List<T>> RetrieveItemsAsync(string partitionKey, string columnName, string columnData, TableContinuationToken continutaionToken)
        {
            List<T> entities = new List<T>();

            var partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            var rowFilter = TableQuery.GenerateFilterCondition(columnName, QueryComparisons.Equal, columnData);

            string combinedFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowFilter);

            do
            {
                var query = new TableQuery<T>().Where(combinedFilter);
                var queryResult = (await GetCloudTable())?.ExecuteQuerySegmented(query, continutaionToken);

                if (queryResult != null && queryResult.Results.Count > 0)
                {
                    entities.AddRange(queryResult.Results);
                }
                continutaionToken = queryResult.ContinuationToken;
            } while (continutaionToken != null);

            return entities;
        }
    }
}


