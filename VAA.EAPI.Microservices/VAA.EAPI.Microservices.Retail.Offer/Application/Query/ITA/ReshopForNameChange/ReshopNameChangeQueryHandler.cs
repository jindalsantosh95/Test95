using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.ReshopNameChange;
using VAA.EAPI.Microservices.Retail.Offer.Validation.Ita;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.ReshopForNameChange
{
    public class ReshopNameChangeQueryHandler : IReshopNameChange
    {        
        private readonly ILogger<ReshopNameChangeQueryHandler> _logger;
        private readonly IDistributedCacheService _distributedCacheService;

        public ReshopNameChangeQueryHandler(ILogger<ReshopNameChangeQueryHandler> logger, ICachedPriceClass cachedPriceClass, ICachedCabin cachedCabin, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, ICachedAirportDetails cachedAirportDetails, IDistributedCacheService distributedCacheService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));           
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }
        public async Task<Result<Response<OfferResponse>>> Handle(ReshopRequest requestData, CancellationToken cancellationToken)
        {
            try
            {
                var result = OrderReshopRequestValidator.ValidatorOrderReshopNameChangeRequest(requestData);
                if (result.Value.Status == Status.OK)
                {
                    try
                    {
                        string docId = Guid.NewGuid().ToString();
                        string docType = Constants.Application.ReshopNameChangeDocType;
                        var reshopResponse = await ReshopNameChangeResponseMapper.ReshopResponseMapper(requestData, docId, DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)).ToString(Constants.Application.ZuluDateTimeFormat));

                        var redisOfferDocument = NameChangeOfferDocumentMapper.RedisDocumentMapper(docId, docType, requestData, reshopResponse);
                        await _distributedCacheService.Set(docId, redisOfferDocument, false, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)));

                        return new Response<OfferResponse>
                        {
                            Status = Status.OK,
                            Value = reshopResponse
                        };
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogError(ex, $"Error occurred in Order Reshop NameChange Query Handler");
                        return Result.Failure<Response<OfferResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.ITAConnectionCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                    }
                }
                else
                {
                    return Result.Success(new Response<OfferResponse>
                    {
                        Errors = result.Value.Errors,
                        Status = result.Value.Status
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error has occurred in Order Reshop NameChange Query Handler");
                return Result.Failure<Response<OfferResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
