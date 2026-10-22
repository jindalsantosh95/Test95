using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Flight.Services.Domain;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Flight.Services.Mapper;

namespace VAA.EAPI.Microservices.Flight.Services.Application.Query
{
    public class FlightServiceQueryHandler : IRequestHandler<FlightServiceQuery, Result<Response<FlightServiceResponse>>>
    {

        private readonly ICachedSsrDetails _cachedSsrDetails;
        private readonly ILogger<FlightServiceQueryHandler> _logger;
        private readonly IDistributedCacheService _distributedCacheService;

        public FlightServiceQueryHandler(ICachedSsrDetails cachedSSRDetails, ILogger<FlightServiceQueryHandler> logger, IDistributedCacheService distributedCacheService)
        {
            _cachedSsrDetails = cachedSSRDetails;
            _logger = logger;
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task<Result<Response<FlightServiceResponse>>> Handle(FlightServiceQuery request, CancellationToken cancellationToken)
        {
            try
            {
                string docId = Guid.NewGuid().ToString();
                string docType = Constants.Application.FlightServicveDocType;
                var ssrDetails = await _cachedSsrDetails.GetSsrDetails();

                var flightServiceResposne = FlightServiceReponseMapper.ResponseMapper(ssrDetails, request, docId);

                flightServiceResposne.OfferExpirationTimeLimit = DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)).ToString(Constants.Application.ZuluDateTimeFormat);

                var absouluteCacheExpiration = TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes));
                var flightServiceDocument = FlightServiceResponseToRedisCacheMapper.RedisDocumentMapper(flightServiceResposne, docId, docType);

                await _distributedCacheService.Set<FlightServiceResponse>(docId, flightServiceResposne, false, TimeSpan.FromSeconds(0), absouluteCacheExpiration);

                return new Response<FlightServiceResponse>
                {
                    Status = Status.OK,
                    Value = flightServiceResposne
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Flight service query failed for {OrderId}.", request.OrderId);

                return Result.Failure<Response<FlightServiceResponse>>(ex.Message);
            }

        }
    }
}
