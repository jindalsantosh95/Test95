using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Flight.Services.Domain;
using VAA.EAPI.Microservices.Flight.Services.Mapper;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Repositories
{
    public class FlightServiceOfferSearchRepoQueryHandler : IRequestHandler<FlightServiceOfferSearchRepoQuery, FlightServiceResponse>
    {

        private readonly IDistributedCacheService _distributedCacheService;

        public FlightServiceOfferSearchRepoQueryHandler(IDistributedCacheService distributedCacheService)
        {            
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));

        }

        public async Task<FlightServiceResponse> Handle(FlightServiceOfferSearchRepoQuery request, CancellationToken cancellationToken)
        {
            FlightServiceResponse offerDetails;

            offerDetails = await SearchFlightServicerInRedisCache(request, cancellationToken);

            return offerDetails.Offers == null ? new() : offerDetails;
        }

        private async Task<FlightServiceResponse> SearchFlightServicerInRedisCache(FlightServiceOfferSearchRepoQuery request, CancellationToken cancellationToken)
        {
            var flightServiceDocument = await _distributedCacheService.Get<FlightServiceResponseDocument>(request.FlightServiceOfferID);

            return (flightServiceDocument != null) ? (FlightServiceResponse) flightServiceDocument : new FlightServiceResponse();                        
        }
    }
}
