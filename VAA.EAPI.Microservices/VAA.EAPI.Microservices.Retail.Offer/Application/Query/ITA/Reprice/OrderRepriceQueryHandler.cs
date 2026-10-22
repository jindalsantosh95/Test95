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
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reprice;
using VAA.EAPI.Microservices.Retail.Offer.Validation.Ita;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reprice
{
    public class OrderRepriceQueryHandler : IRequestHandler<OrderRepriceQuery, Result<Response<OfferResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderRepriceQueryHandler> _logger;
        private readonly ICachedPriceClass _cachedPriceClass;
        private readonly ICachedCabin _cachedCabin;
        private readonly ICabinDetails _cabinDetail;
        private readonly ICachedAmenities _cachedAmenities;
        private readonly IFareFamily _fareFamily;
        private readonly ICachedPieceAllowance _pieceAllowance;
        private readonly ICachedCabinBaggage _cabinBaggage;
        private readonly IDistributedCacheService _distributedCacheService;

        public OrderRepriceQueryHandler(IMediator mediator, ILogger<OrderRepriceQueryHandler> logger, ICabinDetails cabinDetail, ICachedPriceClass cachedPriceClass, ICachedCabin cachedCabin, ICachedAmenities cachedAmenities, IFareFamily fareFamily, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, IDistributedCacheService distributedCacheService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cabinDetail = cabinDetail ?? throw new ArgumentNullException(nameof(cabinDetail));
            _cachedPriceClass = cachedPriceClass ?? throw new ArgumentNullException(nameof(cachedPriceClass));
            _cachedCabin = cachedCabin ?? throw new ArgumentNullException(nameof(cachedCabin));
            _cachedAmenities = cachedAmenities ?? throw new ArgumentNullException(nameof(cachedAmenities));
            _fareFamily = fareFamily ?? throw new ArgumentNullException(nameof(fareFamily));
            _pieceAllowance = pieceAllowance ?? throw new ArgumentNullException(nameof(pieceAllowance));
            _cabinBaggage = cabinBaggage ?? throw new ArgumentNullException(nameof(cabinBaggage));
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task<Result<Response<OfferResponse>>> Handle(OrderRepriceQuery requestData, CancellationToken cancellationToken)
        {
            try
            {
                var result = ValidateRepriceRequest.OrderRepriceRequestValidator(requestData);

                if (result.Value.Status == Status.OK)
                {
                    try
                    {
                        var cabins = _cabinDetail.GetCabinDetails();
                        var reshopDoc = await _mediator.Send(new ShoppingDocumentRepoQuery(requestData.PricedOffer.SelectedOffer.OfferRefID), cancellationToken);

                        if (reshopDoc.Offers == null)
                        {
                            return Result.Success(new Response<OfferResponse>
                            {
                                Status = Status.BusinessException,
                                Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidOfferRefId), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                            });
                        }

                        var fareFamilyName = await _fareFamily.GetFareFamily(reshopDoc.DataList.PriceClass.First().ProductName);
                        var searchResult = RepriceRequestMapper.RepriceSearchReqMapper(requestData, fareFamilyName, cabins, reshopDoc.ShoppingResponseId);
                        var response = await _mediator.Send(new ItaRepoQuery { Search = searchResult }, cancellationToken);

                        if (response.messages?.error != null || response.error != null)
                        {
                            var results = GetITAErrorsFromResponse.GetITAErrors(response, true);
                            if (results.Value.Status != Status.OK)
                            {
                                _logger.LogError($"Error occurred in OrderRepriceQueryHandler in ITA Service : {results.Value.Errors?.First().DeveloperMessage}");

                                Status status = results.Value.Errors.First().DeveloperMessage.Contains(Constants.Application.ITAError) ? Status.Error : results.Value.Status;
                                return Result.Success(new Response<OfferResponse>
                                {
                                    Errors = results.Value.Errors,
                                    Status = status
                                });
                            }
                        }

                        if (ValidateRepriceSelectedId(response))
                        {
                            return Result.Success(new Response<OfferResponse>
                            {
                                Status = Status.BusinessException,
                                Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.RepriceNoSelectedIdCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                            });
                        }

                        bool priceChange = CheckPriceChange(reshopDoc.Offers.FirstOrDefault(x => x.OfferId == requestData.PricedOffer.SelectedOffer.OfferRefID), response);
                        var repriceResponse = await RepriceResponseMapper.MaptoRepriceResponse(response, requestData, _cachedPriceClass, _cachedCabin, _mediator, _cachedAmenities, reshopDoc, priceChange, _pieceAllowance, _cabinBaggage);

                        if (priceChange)
                        {
                            var redisOfferDocument = RepriceResponseToDocumentMapper.CreateDocumentMapper(repriceResponse, repriceResponse.Offers.First(x => x.Selected).OfferId, Constants.Application.OfferPriceDocType);
                            await _distributedCacheService.Set(repriceResponse.Offers.First(x => x.Selected).OfferId, redisOfferDocument, false, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)));
                        }
                        else
                        {
                            var offerId = repriceResponse.Offers.First(x => x.Selected).OfferId;
                            var docFetchResponse = await _mediator.Send(new OfferSearchRepoQuery(offerId), cancellationToken);

                            if (docFetchResponse.Offers == null)
                            {
                                return Result.Success(new Response<OfferResponse>
                                {
                                    Status = Status.BusinessException,
                                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidOfferRefId), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                                });
                            }

                            if (docFetchResponse.ShoppingResponseId == reshopDoc.ShoppingResponseId)
                            {
                                var redisOfferDocument = RepriceResponseToDocumentMapper.CreateDocumentMapper(repriceResponse, offerId, GetRepriceDocumentType(reshopDoc.DocType));
                                await _distributedCacheService.Set(offerId, redisOfferDocument, false, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)));
                            }
                            else
                            {
                                repriceResponse.Offers.First(o => o.Selected).OfferExpirationTimeLimit = docFetchResponse.Offers.First().OfferExpirationTimeLimit;
                                var redisOfferDocument = RepriceResponseToDocumentMapper.CreateDocumentMapper(repriceResponse, offerId, Constants.Application.OfferPriceDocType);
                                await _distributedCacheService.Set(offerId, redisOfferDocument, false, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)));
                            }
                        }

                        // Check for upsell offers and store them
                        await StoreUpsellOffers(repriceResponse, cancellationToken);

                        return new Response<OfferResponse>
                        {
                            Status = Status.OK,
                            Value = repriceResponse
                        };
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogError(ex, "Error occurred in Order Reprice Query Handler in ITA Service");
                        return Result.Failure<Response<OfferResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.ITAConnectionCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                    }
                    catch (ExternalServiceIntegrationException ex)
                    {
                        _logger.LogError(ex, "Error occurred in Order Reprice Query Handler in ITA Service");
                        return Result.Failure<Response<OfferResponse>>(Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.RepriceBusinessCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message)));
                    }
                }

                return Result.Success(new Response<OfferResponse>
                {
                    Errors = result.Value.Errors,
                    Status = Status.BusinessException
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error has occurred in Order Reprice Query Handler");
                return Result.Failure<Response<OfferResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

        /// <summary>
        /// Checks if any upsell offers present in reprice response and if exists then stores it in cosmos DB with new unique offer id
        /// </summary>
        /// <param name="repriceResponse"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>reprice response with new unique id for upsell offers</returns>
        private static string GetRepriceDocumentType(string reshopDocumentType)
        {
            return reshopDocumentType == Constants.Application.ReshopDisruptionDocType || reshopDocumentType == Constants.Application.ReshopHoldBookingDocType ? reshopDocumentType : Constants.Application.OfferPriceDocType;
        }

        private async Task StoreUpsellOffers(OfferResponse repriceResponse, CancellationToken cancellationToken)
        {
           await Parallel.ForEachAsync(repriceResponse.Offers.Where(x => x.UpsellOffer), async (upsellOffer, CancellationToken) =>
            {
                var redisOfferDocument = RepriceResponseToDocumentMapper.CreateDocumentMapper(repriceResponse, upsellOffer.OfferId, Constants.Application.OfferPriceDocType);
                await _distributedCacheService.Set(upsellOffer.OfferId, redisOfferDocument, false, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)));
            });
        }

        private static bool ValidateRepriceSelectedId(result itaResponse)
        {
            if (itaResponse.Items is not { Length: > 0 })
            {
                return true;
            }

            var tripDetails = itaResponse.Items.FirstOrDefault(x => x.GetType() == typeof(resultTripDetails)) as resultTripDetails;

            return tripDetails?.solution != null && tripDetails.solution.Any() && !tripDetails.solution.Any(soln => soln.ext.selected);
        }

        private static bool CheckPriceChange(OfferDetails selectedReshopOffer, result itaResponse)
        {
            var reshopBasePrice = selectedReshopOffer.SalePrice.First(p => p.PriceComponentType == PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount;
            var itaBasePrice = string.Empty;

            if (itaResponse.Items != null && itaResponse.Items.Length > 0)
            {
                var tripDetails = itaResponse.Items.FirstOrDefault(x => x.GetType() == typeof(resultTripDetails)) as resultTripDetails;

                if (tripDetails?.solution != null && tripDetails.solution.Any())
                {
                    itaBasePrice = tripDetails.solution.First(x => x.ext.selected).saleTotal.amount.ToString();
                }
            }

            return reshopBasePrice != itaBasePrice;
        }
    }
}


