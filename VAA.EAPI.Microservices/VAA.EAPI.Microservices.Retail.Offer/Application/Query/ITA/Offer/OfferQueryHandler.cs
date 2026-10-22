using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common;
using VAA.EAPI.Microservices.Retail.Offer.Validation.Ita;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Offer
{
    public class OfferQueryHandler : IRequestHandler<OfferQuery, Result<Response<OfferResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ICachedPriceClass _cachedPriceClass;
        private readonly ICachedAirportDetails _cachedAirportDetails;
        private readonly ICachedCabin _cachedCabin;
        private readonly ILogger<OfferQueryHandler> _logger;
        private readonly ICachedPieceAllowance _pieceAllowance;
        private readonly ICachedCabinBaggage _cabinBaggage;
        private readonly IDistributedCacheService _distributedCacheService;

        public OfferQueryHandler(IMediator mediator, ICachedPriceClass cachedPriceClass, ICachedCabin cachedCabin, ILogger<OfferQueryHandler> logger, ICachedPieceAllowance pieceAllowance, ICachedCabinBaggage cabinBaggage, ICachedAirportDetails cachedAirportDetails, IDistributedCacheService distributedCacheService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _cachedPriceClass = cachedPriceClass ?? throw new ArgumentNullException(nameof(cachedPriceClass));
            _cachedCabin = cachedCabin ?? throw new ArgumentNullException(nameof(cachedCabin));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pieceAllowance = pieceAllowance ?? throw new ArgumentNullException(nameof(pieceAllowance));
            _cabinBaggage = cabinBaggage ?? throw new ArgumentNullException(nameof(cabinBaggage));
            _cachedAirportDetails = cachedAirportDetails ?? throw new ArgumentNullException(nameof(cachedAirportDetails));
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task<Result<Response<OfferResponse>>> Handle(OfferQuery requestData, CancellationToken cancellationToken)
        {
            string docId = Guid.NewGuid().ToString();

            var searchResult = OfferITARequestMapper.OfferSearchReqMapper(requestData, true);
            var offerExpiryLimit = DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes)).ToString(Constants.Application.ZuluDateTimeFormat);
            var shoppingOfferResponse = OfferITAResponseMapper.CreateStreamingResponse(requestData);
            var dataList = OfferITAResponseMapper.CreateStreamingDataList(requestData);
            var streamedOffers = new List<OfferDetails>();

            var response = await _mediator.Send(new ItaStreamingRepoQuery
            {
                Search = searchResult,
                SolutionHandler = async (solution, solutionCancellationToken) =>
                {
                    solutionCancellationToken.ThrowIfCancellationRequested();
                    var offer = await OfferITAResponseMapper.MapStreamingSolution(
                        solution,
                        requestData,
                        dataList,
                        docId,
                        offerExpiryLimit,
                        _cachedCabin,
                        _cachedPriceClass,
                        _pieceAllowance,
                        _cabinBaggage,
                        _cachedAirportDetails);

                    if (offer != null)
                    {
                        streamedOffers.Add(offer);
                    }
                }
            }, cancellationToken);

            if (response.messages?.error != null || response.error != null)
            {
                var results = GetITAErrorsFromResponse.GetITAErrors(response);
                if (results.Value.Status != Status.OK)
                {
                    _logger.LogError($"Error occurred in OfferQueryHandler in ITA Service : {results.Value.Errors?.First().DeveloperMessage}");

                    Status status = results.Value.Errors.First().DeveloperMessage.Contains(Constants.Application.ITAError) ? Status.Error : results.Value.Status;
                    return Result.Success(new Response<OfferResponse>
                    {
                        Errors = results.Value.Errors,
                        Status = status
                    });
                }
            }

            shoppingOfferResponse.Offers = streamedOffers;
            OfferITAResponseMapper.FinalizeStreamingResponse(shoppingOfferResponse, dataList, response);

            if (shoppingOfferResponse.Offers != null && shoppingOfferResponse.Offers.Any())
            {
                var offerResponse = ShoppingOfferDcoumentMapper.ShoppingOfferDcoument(shoppingOfferResponse);
                var cacheAbsoluteExpiration = TimeSpan.FromMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes));
                var redisOfferDocument = OfferResponseToOfferDocumentMapper.RedisDocumentMapper(offerResponse, docId, Constants.Application.OfferDocType, null);

                await _distributedCacheService.Set(docId, redisOfferDocument, false, TimeSpan.FromSeconds(0), cacheAbsoluteExpiration);

                return new Response<OfferResponse>
                {
                    Status = Status.OK,
                    Value = shoppingOfferResponse
                };
            }
            else
            {
                var results = GetITAErrorsFromResponse.GetITAErrors(response);

                var accountCode = requestData.Parties.Participants?.FirstOrDefault()?.AccountCode;
                var hasWarnings = response?.messages?.warning?.Any() == true;
                var hasNoSolution = response?.solutionCountSpecified != true || response.solutionCount == 0;

                if (shoppingOfferResponse.Offers == null && (!string.IsNullOrWhiteSpace(accountCode) || (hasNoSolution && !hasWarnings)))
                {
                    return Result.Success(new Response<OfferResponse>
                    {
                        Status = Status.BusinessException,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.OfferSearchNoOffers), Guid.NewGuid(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    });
                }

                return Result.Success(new Response<OfferResponse>
                {
                    Errors = results.Value.Errors,
                    Status = Status.BusinessException
                });
            }
        }
    }
}
