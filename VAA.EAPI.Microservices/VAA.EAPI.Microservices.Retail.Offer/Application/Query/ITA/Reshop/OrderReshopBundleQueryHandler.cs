using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Validation.Ita;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reshop
{
    public class OrderReshopBundleQueryHandler : IRequestHandler<OrderReshopBundleQuery, Result<Response<OfferResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderReshopBundleQueryHandler> _logger;
        private readonly ICachedPriceClass _cachedPriceClass;
        private readonly ICachedAirportDetails _cachedAirportDetails;
        private readonly ICachedCabin _cachedCabin;
        private readonly ICachedPieceAllowance _pieceAllowance;
        private readonly ICachedCabinBaggage _cabinBaggage;
        private readonly IDistributedCacheService _distributedCacheService;

        public OrderReshopBundleQueryHandler(IMediator mediator, ILogger<OrderReshopBundleQueryHandler> logger, ICachedPriceClass cachedPriceClass, ICachedCabin cachedCabin, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, ICachedAirportDetails cachedAirportDetails, IDistributedCacheService distributedCacheService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cachedPriceClass = cachedPriceClass ?? throw new ArgumentNullException(nameof(cachedPriceClass));
            _cachedAirportDetails = cachedAirportDetails ?? throw new ArgumentNullException(nameof(cachedAirportDetails));
            _cachedCabin = cachedCabin ?? throw new ArgumentNullException(nameof(cachedCabin));
            _pieceAllowance = pieceAllowance ?? throw new ArgumentNullException(nameof(pieceAllowance));
            _cabinBaggage = cabinBaggage ?? throw new ArgumentNullException(nameof(cabinBaggage));
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }
        public async Task<Result<Response<OfferResponse>>> Handle(OrderReshopBundleQuery requestData, CancellationToken cancellationToken)
        {
            try
            {
                var result = OrderReshopRequestValidator.ValidatorOrderReshopRequest(requestData);
                if (result.Value.Status == Status.OK)
                {
                    try
                    {
                        string docId = Guid.NewGuid().ToString();
                        string docType = requestData.IsReshopHoldBooking ? Constants.Application.ReshopHoldBookingDocType : requestData.IsReshopDisruption ? Constants.Application.ReshopDisruptionDocType : Constants.Application.ReshopDocType;
                        var searchResult = ReshopRequestMapper.ReshopSearchReqMapper(requestData, true);
                        var response = await _mediator.Send(new ItaRepoQuery { Search = searchResult });

                        if (response.messages?.error != null || response.error != null || (response.messages?.warning?.Any(x => x.code == 201) == true))
                        {
                            var results = GetITAErrorsFromResponse.GetITAErrors(response);
                            if (results.Value.Status != Status.OK)
                            {
                                _logger.LogError($"Error occurred in OrderReshopBundleQueryHandler in ITA Service : {results.Value.Errors?.First().DeveloperMessage}");

                                Status status = results.Value.Errors.First().DeveloperMessage.Contains(Constants.Application.ITAError) ? Status.Error : results.Value.Status;
                                return Result.Success(new Response<OfferResponse>
                                {
                                    Errors = results.Value.Errors,
                                    Status = status
                                });
                            }
                        }

                        var reshopResponse = await ReshopBundleResponseMapper.ReshopResponseMapper(searchResult, response, requestData, _cachedPriceClass, _cachedCabin, docId, DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)).ToString(Constants.Application.ZuluDateTimeFormat), _pieceAllowance, _cabinBaggage, _cachedAirportDetails);

                        if (reshopResponse.Offers.Any())
                        {
                            var redisOfferDocument = OfferResponseToOfferDocumentMapper.RedisDocumentMapper(reshopResponse, docId, docType, requestData);
                            await _distributedCacheService.Set(docId, redisOfferDocument, false, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)));
                        }

                        return new Response<OfferResponse>
                        {
                            Status = Status.OK,
                            Value = reshopResponse
                        };
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogError(ex, $"Error occurred in Order Reshop Query Handler in ITA Service");
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
                _logger.LogError(ex, "An unexpected error has occurred in Order Reshop Bundle Query Handler");
                return Result.Failure<Response<OfferResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
