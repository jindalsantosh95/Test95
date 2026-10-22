using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Caching;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common
{
    public class OfferDeleteRepoCommandHandler : IRequestHandler<OfferDeleteRepoCommand, Result<Unit>>
    {
        private readonly IDistributedCacheService _distributedCacheService;

        public OfferDeleteRepoCommandHandler(IDistributedCacheService distributedCacheService)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task<Result<Unit>> Handle(OfferDeleteRepoCommand request, CancellationToken cancellationToken)
        {
            await _distributedCacheService.Remove(request.CosmosDocument.Id);

            return Unit.Value;
        }
    }
}
