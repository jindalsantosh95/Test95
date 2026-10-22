using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Core.Xunit.Azure
{
    public sealed class TableStorageService<T> : ITableStorageService<T> where T : TableEntity, ITableEntity, new()
    {
        private readonly ConcurrentDictionary<string, T> _items = [];

        public string TableName { get; set; }
        public string ConnectionString { get; set; }
        public int Count => _items.Count;
        public IReadOnlyDictionary<string, T> Items => _items;

        public TableStorageService() { }
        public TableStorageService(string connectionString, string tableName)
        {
            ConnectionString = connectionString;
            TableName = tableName;
        }

        public async Task<T> DeleteAsync(T entity) => _items.TryRemove(entity.RowKey, out T item) ? item : default(T);

        public async Task<T> InsertOrMergeAsync(T entity)
        {
            if (_items.TryGetValue(entity.RowKey, out T currentEntity))
            {
                var merged = JObject.Parse(Json.Serialize(currentEntity));
                var update = JObject.Parse(Json.Serialize(entity));

                merged.Merge(update, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });

                var newEntity = Json.Deserialize<T>(merged.ToString());

                if (_items.TryUpdate(entity.RowKey, newEntity, currentEntity))
                    return newEntity;

                throw new InvalidOperationException("Failed to merge and store entity");
            }
            else
                return _items.GetOrAdd(entity.RowKey, entity);
        }

        public async Task<T> RetrieveAsync(string partitionKey, string rowKey)
            => _items.Any(x => x.Value.PartitionKey.Is(partitionKey) && x.Key.Is(rowKey)) ? _items.First(x => x.Value.PartitionKey.Is(partitionKey) && x.Key.Is(rowKey)).Value : default(T);

        public async Task<List<T>> RetrieveItemsAsync(TableContinuationToken continutaionToken) => _items.Values.ToList();

        public async Task<List<T>> RetrieveItemsAsync(string partitionKey, string columnName, string columnData, TableContinuationToken continuationToken)
            => [.. _items.Values.Where(x => x.PartitionKey.Is(partitionKey) && x.PropertyIs(columnName, columnData))];

        public async Task<List<T>> RetrieveItemsInPartitionAsync(string partitionKey, TableContinuationToken continutaionToken)
            => [.. _items.Values.Where(x => x.PartitionKey.Is(partitionKey))];
    }
}
