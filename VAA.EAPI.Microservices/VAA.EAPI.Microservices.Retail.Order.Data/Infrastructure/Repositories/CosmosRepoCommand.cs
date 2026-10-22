using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public class CosmosRepoCommand : IRequest<Result<Unit>>
    {
        public CosmosDocumentEvent Event { get; }
        public bool NDC { get; set; }

        public CosmosRepoCommand(CosmosDocumentEvent cosmosEvent , bool ndc = false)
        {
            Event = cosmosEvent;
            NDC = ndc;
        }
    }
}
