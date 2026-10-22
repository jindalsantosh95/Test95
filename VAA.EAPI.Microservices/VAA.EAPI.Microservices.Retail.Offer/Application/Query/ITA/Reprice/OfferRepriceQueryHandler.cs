using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reprice;
using VAA.EAPI.Microservices.Retail.Offer.Validation.Ita;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reprice
{
    public class OfferRepriceQueryHandler : IRequestHandler<OfferRepriceQuery, Result<Response<OfferResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OfferRepriceQueryHandler> _logger;
        private readonly ICabinDetails _cabinDetail;
        private readonly ICachedPriceClass _cachedPriceClass;
        private readonly ICachedCabin _cachedCabin;
        private readonly ICachedAmenities _cachedAmenities;
        private readonly IFareFamily _fareFamily;
        private readonly ICachedPieceAllowance _pieceAllowance;
        private readonly ICachedCabinBaggage _cabinBaggage;
        private readonly IDistributedCacheService _distributedCacheService;
        public OfferRepriceQueryHandler(IMediator mediator, ILogger<OfferRepriceQueryHandler> logger, ICabinDetails cabinDetails, ICachedPriceClass cachedPriceClass, ICachedCabin cachedCabin, ICachedAmenities cachedAmenities, IFareFamily fareFamily, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, IDistributedCacheService distributedCacheService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cabinDetail = cabinDetails ?? throw new ArgumentNullException(nameof(cabinDetails));
            _cachedPriceClass = cachedPriceClass ?? throw new ArgumentNullException(nameof(cachedPriceClass));
            _cachedCabin = cachedCabin ?? throw new ArgumentNullException(nameof(cachedCabin));
            _cachedAmenities = cachedAmenities ?? throw new ArgumentNullException(nameof(cachedAmenities));
            _fareFamily = fareFamily ?? throw new ArgumentNullException(nameof(fareFamily));
            _pieceAllowance = pieceAllowance ?? throw new ArgumentNullException(nameof(pieceAllowance));
            _cabinBaggage = cabinBaggage ?? throw new ArgumentNullException(nameof(cabinBaggage));
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }
        public async Task<Result<Response<OfferResponse>>> Handle(OfferRepriceQuery requestData, CancellationToken cancellationToken)
        {

            var offerDoc = await _mediator.Send(new ShoppingDocumentRepoQuery(requestData.PricedOffer.SelectedOffer.OfferRefID), cancellationToken);

            if (offerDoc is null || offerDoc.Offers.IsNullOrEmpty() || offerDoc.Offers.First().OfferExpirationTimeLimit.Parse<DateTimeOffset>().UtcDateTime < DateTime.UtcNow)
            {
                return Result.Success(new Response<OfferResponse>
                {
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.InvalidOfferRefId, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                    Status = Status.BusinessException
                });
            }

            var fareFamilyName = await _fareFamily.GetFareFamily(offerDoc.DataList.PriceClass.First().ProductName);
            var cabins = _cabinDetail.GetCabinDetails();

            var searchResult = OfferRepriceITARequestMapper.RepriceSearchReqMapper(requestData, fareFamilyName, cabins, offerDoc.ShoppingResponseId,offerDoc.DataList.CustomerDetails);

            var response = await _mediator.Send(new ItaRepoQuery { Search = searchResult }, cancellationToken);
            if (response.messages?.error != null || response.error != null)
            {
                var results = GetITAErrorsFromResponse.GetITAErrors(response, true);
                if (results.Value.Status != Status.OK)
                {
                    _logger.LogError($"Error occurred in OfferRepriceQueryHandler in ITA Service : {results.Value.Errors?.First().DeveloperMessage}");

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

            bool priceChange = CheckPriceChange(offerDoc.Offers.FirstOrDefault(x => x.OfferId == requestData.PricedOffer.SelectedOffer.OfferRefID), response);

            var offerRepriceResponse = await OfferRepriceITAResponseMapper.MaptoRepriceResponse(response, requestData, _cachedPriceClass, _cachedCabin, _mediator, _cachedAmenities, offerDoc, priceChange, _pieceAllowance, _cabinBaggage);

            if (priceChange)
            {
                var redisOfferDocument = RepriceResponseToDocumentMapper.CreateDocumentMapper(offerRepriceResponse, offerRepriceResponse.Offers.First(x => x.Selected).OfferId, Constants.Application.OfferPriceDocType);
                await _distributedCacheService.Set(offerRepriceResponse.Offers.First(x => x.Selected).OfferId, redisOfferDocument, false, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)));
            }
            else
            {
                var offerId = offerRepriceResponse.Offers.First(x => x.Selected).OfferId;
                var docFetchResponse = await _mediator.Send(new OfferSearchRepoQuery(offerId), cancellationToken);

                if (docFetchResponse.ShoppingResponseId == offerDoc.ShoppingResponseId)
                {
                    var redisOfferDocument = RepriceResponseToDocumentMapper.CreateDocumentMapper(offerRepriceResponse, offerId, Constants.Application.OfferPriceDocType);
                    await _distributedCacheService.Set(offerId, redisOfferDocument, false, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)));
                }
                else
                {
                    offerRepriceResponse.Offers.First(o => o.Selected).OfferExpirationTimeLimit = docFetchResponse.Offers.First().OfferExpirationTimeLimit;

                    var redisOfferDocument = RepriceResponseToDocumentMapper.CreateDocumentMapper(offerRepriceResponse, offerId, Constants.Application.OfferPriceDocType);
                    await _distributedCacheService.Set(offerId, redisOfferDocument, false, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)));

                }
            }

            // Check for upsell offers and store them
            await StoreUpsellOffers(offerRepriceResponse, cancellationToken);

            return new Response<OfferResponse>
            {
                Status = Status.OK,
                Value = offerRepriceResponse
            };

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

        private async Task StoreUpsellOffers(OfferResponse repriceResponse, CancellationToken cancellationToken)
        {
            await Parallel.ForEachAsync(repriceResponse.Offers.Where(x => x.UpsellOffer), async (upsellOffer, CancellationToken) =>
            {

                var redisOfferDocument = RepriceResponseToDocumentMapper.CreateDocumentMapper(repriceResponse, upsellOffer.OfferId, Constants.Application.OfferPriceDocType);
                await _distributedCacheService.Set(upsellOffer.OfferId, redisOfferDocument, false, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)));
            });
        }

        private static bool CheckPriceChange(OfferDetails selectedOffer, result itaResponse)
        {
            var reshopBasePrice = selectedOffer.SalePrice.First(p => p.PriceComponentType == PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount;
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

