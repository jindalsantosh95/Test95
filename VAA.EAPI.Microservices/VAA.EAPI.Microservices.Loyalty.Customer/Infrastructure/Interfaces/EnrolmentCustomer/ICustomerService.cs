using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer
{
    public interface ICustomerService<T> where T : CosmosDocument
    {
        string DatabaseName { get; set; }
        string CollectionName { get; set; }
        string PartitionKey { get; set; }

        /// <summary>
        /// To create a Document
        /// </summary>
        /// <param name="item">Document</param>
        /// <returns></returns>
        Task<T> CreateItemAsync(T item, string partitionKey, DocumentType cosmosCollectionEnum);

        Task<List<T>> GetItemAsync(DocumentType cosmosCollectionEnum, string sqlQueryText, string MemberId);
        /// <summary>
        /// Replace existing Document with specified Partition Key and preserves partition Key
        /// </summary>
        /// <returns></returns>
        Task<T> ReplaceItemAsync(T item, DocumentType cosmosCollectionEnum);
    }
}
