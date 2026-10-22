using Microsoft.Azure.Cosmos;
using VAA.EAPI.Infrastructure.Entities.Enums;

namespace VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2
{
    public class CosmosDocumentEvent
    {
        public CosmosDocument Document { get; set; }
        public CosmosModificationType ModificationType { get; set; }
        public ItemRequestOptions RequestOptions { get; set; }
        public CosmosDocumentEvent(CosmosDocument dto, CosmosModificationType modType)
        {
            Document = dto;
            ModificationType = modType;
        }

        public CosmosDocumentEvent(CosmosDocument dto, CosmosModificationType modType, ItemRequestOptions requestOptions)
        {
            Document = dto;
            ModificationType = modType;
            RequestOptions = requestOptions;
        }

        public CosmosDocumentEvent()
        {
            // Required for serialization
        }
    }
}