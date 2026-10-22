using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common
{
    public sealed class CancelReshopOfferSearchRepoQueryHandler : IRequestHandler<CancelReshopOfferSearchRepoQuery, CancelReshopResponse>
    {
        private readonly IDistributedCacheService _distributedCacheService;

        public CancelReshopOfferSearchRepoQueryHandler(IDistributedCacheService distributedCacheService)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task<CancelReshopResponse> Handle(CancelReshopOfferSearchRepoQuery request, CancellationToken cancellationToken)
        {
            var offerDocumentId = request.OfferId;
            var offerDocument = await _distributedCacheService.Get<CancelReshopResponse>(request.OfferId);
            
            return offerDocument;
        }
    }
}
