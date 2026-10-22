using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories
{
    public class CosmosRepoCommand : IRequest<Result<Unit>>
    {
        public CosmosDocumentEvent Event { get; }
       
        public CosmosRepoCommand(CosmosDocumentEvent cosmosEvent)
        {
            Event = cosmosEvent;
        }
    }
}
