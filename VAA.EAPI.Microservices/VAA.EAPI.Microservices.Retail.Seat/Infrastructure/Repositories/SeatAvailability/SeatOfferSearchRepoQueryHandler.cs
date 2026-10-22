using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability
{
    public class SeatOfferSearchRepoQueryHandler : IRequestHandler<SeatOfferSearchRepoQuery, SeatAvailabilityResponse>
    {
        private readonly IDistributedCacheService _distributedCacheService;

        public SeatOfferSearchRepoQueryHandler(IDistributedCacheService distributedCacheService)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task<SeatAvailabilityResponse> Handle(SeatOfferSearchRepoQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"{request.SeatOfferID}";
            var seatAvailabilityDocument = await _distributedCacheService.Get<SeatAvailabilityDocument>(cacheKey);

            SeatAvailabilityResponse offerDetails = seatAvailabilityDocument != null ? (SeatAvailabilityResponse)seatAvailabilityDocument : new SeatAvailabilityResponse();

            return offerDetails.FlightSegmentSeatMap == null ? new() : offerDetails;
        }
    }
}
