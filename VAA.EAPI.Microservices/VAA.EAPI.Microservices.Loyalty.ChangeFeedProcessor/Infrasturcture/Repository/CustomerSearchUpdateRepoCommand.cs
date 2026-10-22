using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Repository
{
    public class CustomerSearchUpdateRepoCommand : IRequest<Result<Unit>>
    {
        public CosmosDocumentEvent Event { get; }

        public CustomerSearchUpdateRepoCommand(CosmosDocumentEvent cosmosEvent)
        {
            Event = cosmosEvent;
        }
    }
}
