using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain.Enum;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Infrastructure.Interfaces
{
    public interface IOrderService<T> where T : CosmosDocument
    {

        /// create  document with specified Partition Key and preserves partition Key
        /// </summary>
        /// <returns></returns>
        Task<T> InsertItemAsync(T item, DocumentType cosmosCollectionEnum);

        /// <summary>
        /// update  document with specified Partition Key and preserves partition Key
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cosmosCollectionEnum"></param>
        /// <returns></returns>
        Task<T> PatchItemAsync(T item, DocumentType cosmosCollectionEnum);

        /// <summary>
        /// Get for document with FC number
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cosmosCollectionEnum"></param>
        /// <returns></returns>
        Task<List<T>> GetItemAsync(string orderId, string docType);

        /// <summary>
        /// check for document with FCNumber
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cosmosCollectionEnum"></param>
        /// <returns></returns>
        Task<bool> Exists(string orderId, string docType);

        Task DeleteItemAsync(string orderId, string customerId, string id, string PartitionKey, string docType);
    }
}
