
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.AzureStorage
{
    public interface ITableStorageService<T> where T : ITableEntity
    {
        string TableName { get; set; }
        string ConnectionString { get; set; }

        Task<T> RetrieveAsync(string partitionKey, string rowKey);
        Task<T> InsertOrMergeAsync(T entity);
        Task<T> DeleteAsync(T entity);
        Task<List<T>> RetrieveItemsAsync(TableContinuationToken continutaionToken);
        Task<List<T>> RetrieveItemsAsync(string partitionKey, string columnName, string columnData, TableContinuationToken continutaionToken);
        Task<List<T>> RetrieveItemsInPartitionAsync(string partitionKey, TableContinuationToken continutaionToken);
    }
}
