using MediatR;
using Newtonsoft.Json;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain.Enum;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Infrastructure.Services
{
    public class OrderService<T> : IOrderService<T> where T : CosmosDocument
    {
        private readonly IMediator _mediator;

        public OrderService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<T> InsertItemAsync(T item, DocumentType cosmosCollectionEnum)
        {
            var cosmosModifyRepoCommand = new CosmosModifyRepoCommand()
            {
                CollectionName = ConfigReader.Get(Constants.Environment.SingleOrderViewDBCollection),
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.SingleOrderViewDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.SingleOrderViewDBName),

                Event = new CosmosDocumentEvent()
                {
                    Document = item,
                    ModificationType = CosmosModificationType.Create
                }
            };

            await _mediator.Send(cosmosModifyRepoCommand);

            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<bool> Exists(string orderId, string docType)
        {
            List<KeyValuePair<string, object>> parameters = null;
            List<T> currentResultSet = new();

            parameters = new()
                {
                    new KeyValuePair<string, object>("DocType",docType),
                    new KeyValuePair<string, object>("OrderID", orderId)
                };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SingleOrderViewDBCollection),
                                                              ConfigReader.Get(Constants.Environment.SingleOrderViewDBEndPoint),
                                                              ConfigReader.Get(Constants.Environment.SingleOrderViewDBName),
                                                              parameters,
                                                              Constants.Application.SqlQuery));

            return customerSearchResponse != null && customerSearchResponse.Any();
        }

        public async Task<List<T>> GetItemAsync(string orderId, string docType)
        {

            List<KeyValuePair<string, object>> parameters = new()
        {
            new KeyValuePair<string, object>("DocType", docType),
            new KeyValuePair<string, object>("OrderID", orderId)
        };

            var response = await _mediator.Send(
                CosmosQueryHandlerMapper.Mapper(
                    ConfigReader.Get(Constants.Environment.SingleOrderViewDBCollection),
                    ConfigReader.Get(Constants.Environment.SingleOrderViewDBEndPoint),
                    ConfigReader.Get(Constants.Environment.SingleOrderViewDBName),
                    parameters,
                    Constants.Application.SqlQuery));

            if (response != null && response.Count > 0)
            {

                return response.Select(x => JsonConvert.DeserializeObject<T>(x.ToString())).ToList();

            }

            return new List<T>();


        }

        public async Task<T> PatchItemAsync(T item, DocumentType cosmosCollectionEnum)
        {
            var cosmosModifyRepoCommand = new CosmosModifyRepoCommand()
            {
                CollectionName = ConfigReader.Get(Constants.Environment.SingleOrderViewDBCollection),
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.SingleOrderViewDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.SingleOrderViewDBName),

                Event = new CosmosDocumentEvent()
                {
                    Document = item,
                    ModificationType = CosmosModificationType.Replace
                }
            };

            await _mediator.Send(cosmosModifyRepoCommand);

            return item;
        }

        public async Task DeleteItemAsync(string orderId, string customerId, string id, string PartitionKey, string docType)
        {

            var cosmosModifyRepoCommand = new CosmosModifyRepoCommand()
            {
                CollectionName = ConfigReader.Get(Constants.Environment.SingleOrderViewDBCollection),
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.SingleOrderViewDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.SingleOrderViewDBName),

                Event = new CosmosDocumentEvent()
                {
                    Document = new CosmosDocument
                    {
                        Id = id,
                        PartitionKey = PartitionKey


                    },
                    ModificationType = CosmosModificationType.Delete
                }
            };

            await _mediator.Send(cosmosModifyRepoCommand);


        }
    }

}
