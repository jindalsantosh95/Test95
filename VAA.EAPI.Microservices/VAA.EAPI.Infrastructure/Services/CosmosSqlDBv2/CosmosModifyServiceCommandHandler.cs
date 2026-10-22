using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2
{
    public class CosmosModifyServiceCommand : CosmosServiceBase, IRequest<ItemResponse<CosmosDocument>>
    {
        public CosmosDocumentEvent Event { get; set; }
    }
    public class CosmosModifyServiceCommandHandler : IRequestHandler<CosmosModifyServiceCommand, ItemResponse<CosmosDocument>>
    {
        private readonly ILogger<CosmosModifyServiceCommandHandler> _logger;
        private readonly ICosmosClientFactory _cosmosFactory;

        public CosmosModifyServiceCommandHandler(ILogger<CosmosModifyServiceCommandHandler> logger, ICosmosClientFactory cosmosFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cosmosFactory = cosmosFactory ?? throw new ArgumentNullException(nameof(cosmosFactory));
        }

        public async Task<ItemResponse<CosmosDocument>> Handle(CosmosModifyServiceCommand request, CancellationToken cancellationToken)
        {
            var trace = _logger.CreateTrace();

            try
            {
                CosmosClient cosmosClient = _cosmosFactory.GetClient(request.DatabaseEndpoint);

                trace.Diagnostics.LogInformation();

                switch (request.Event.ModificationType)
                {
                    case CosmosModificationType.Create:
                        return await cosmosClient.GetContainer(request.DatabaseName, request.CollectionName).CreateItemAsync(request.Event.Document, new PartitionKey(request.Event.Document.PartitionKey));
                    case CosmosModificationType.Update:
                        {
                            if (request.Event is not CosmosDocumentUpdateEvent)
                                throw new ArgumentException("Invalid request.Event. Expecting 'CosmosDocumentUpdateEvent'");
                            return await cosmosClient.GetContainer(request.DatabaseName, request.CollectionName).PatchItemAsync<CosmosDocument>(request.Event.Document.Id, new PartitionKey(request.Event.Document.PartitionKey), ((CosmosDocumentUpdateEvent)request.Event)._operations);
                        }
                    case CosmosModificationType.Replace:
                        return await cosmosClient.GetContainer(request.DatabaseName, request.CollectionName).ReplaceItemAsync(request.Event.Document, request.Event.Document.Id, new PartitionKey(request.Event.Document.PartitionKey), request.Event.RequestOptions);
                    case CosmosModificationType.Delete:
                        return await cosmosClient.GetContainer(request.DatabaseName, request.CollectionName).DeleteItemAsync<CosmosDocument>(request.Event.Document.Id, new PartitionKey(request.Event.Document.PartitionKey));
                    case CosmosModificationType.Upsert:
                        return await cosmosClient.GetContainer(request.DatabaseName, request.CollectionName).UpsertItemAsync<CosmosDocument>(request.Event.Document, new PartitionKey(request.Event.Document.PartitionKey), cancellationToken: cancellationToken);
                    default:
                        throw new NotImplementedException("Cosmos Modification Type not supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating (Create/Patch/Delete) document in Cosmos DB");
                throw;
            }
            finally
            {
                trace.Diagnostics.LogInformation();
            }
        }
    }
}
