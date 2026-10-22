using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common
{
    public sealed class OfferSearchRepoQueryHandler : IRequestHandler<OfferSearchRepoQuery, OfferResponse>
    {
        private readonly IDistributedCacheService _distributedCacheService;

        public OfferSearchRepoQueryHandler(IDistributedCacheService distributedCacheService)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task<OfferResponse> Handle(OfferSearchRepoQuery request, CancellationToken cancellationToken)
        {
            var offerDocumentId = request.OfferId.Split(Constants.Application.Pipeline)[0];
            var offerDocument = await _distributedCacheService.Get<OfferDocument>(request.OfferId) ?? await _distributedCacheService.Get<OfferDocument>(offerDocumentId);

            if (offerDocument != null)
                offerDocument.FlightOfferResponse.Offers = offerDocument.FlightOfferResponse.Offers.Where(x => x.OfferId == request.OfferId);

            return offerDocument?.FlightOfferResponse.Offers == null ? new() : OfferDocumentToOfferResponseMapper.Map(offerDocument);
        }
    }
}
