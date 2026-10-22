using CSharpFunctionalExtensions;
using MediatR;
using System.Collections;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public class CosmosUpdateRepoCommand : IRequest<Result<Unit>>
    {
        public CosmosDocumentUpdateEvent Event { get; }

        public bool IsInsurancePurchaseDetailsUpdated { get; }

        public CosmosUpdateRepoCommand(CosmosDocumentUpdateEvent cosmosEvent, bool isInsurancePurchaseDetailsUpdated)      
        {
            Event = cosmosEvent;
            Event.ModificationType = EAPI.Infrastructure.Entities.Enums.CosmosModificationType.Update;
            if (isInsurancePurchaseDetailsUpdated)
            {
                Event.AddField("/InsurancePurchaseDetails", ((OrderDocument)cosmosEvent.Document).InsurancePurchaseDetails);
            }           
        }
    }
}
