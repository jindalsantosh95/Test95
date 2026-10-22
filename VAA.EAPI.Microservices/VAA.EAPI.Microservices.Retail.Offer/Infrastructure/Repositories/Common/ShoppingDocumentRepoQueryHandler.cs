using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common
{    
    public sealed class ShoppingDocumentRepoQueryHandler : IRequestHandler<ShoppingDocumentRepoQuery, OfferResponse>
    {
        private readonly IDistributedCacheService _distributedCacheService;

        public ShoppingDocumentRepoQueryHandler(IDistributedCacheService distributedCacheService)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task<OfferResponse> Handle(ShoppingDocumentRepoQuery request, CancellationToken cancellationToken)
        {
            var shoppingDocId = request.ShoppingSelectedOfferId.Split(Constants.Application.Pipeline)[0];
            var offerDocument = await _distributedCacheService.Get<OfferDocument>(shoppingDocId);

            if (offerDocument != null)
                offerDocument.FlightOfferResponse.Offers = offerDocument.FlightOfferResponse.Offers.Where(x => x.OfferId == request.ShoppingSelectedOfferId);

            return offerDocument?.FlightOfferResponse.Offers == null ? new() : OfferDocumentToOfferResponseMapper.Map(offerDocument);
        }
    }
}
