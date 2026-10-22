using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common
{
    public class OfferDeleteRepoCommand : IRequest<Result<Unit>>
    {
        public CosmosDocument CosmosDocument { get; set; }

        public OfferDeleteRepoCommand(CosmosDocument cosmosDocument)
        {
            CosmosDocument = cosmosDocument;
        }
    }
}
