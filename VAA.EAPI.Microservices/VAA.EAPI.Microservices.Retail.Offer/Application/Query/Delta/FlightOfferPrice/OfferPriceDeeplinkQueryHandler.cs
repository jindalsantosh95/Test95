using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta.FlightOfferPrice;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightOfferPrice
{
    public class OfferPriceDeeplinkQueryHandler : IRequestHandler<OfferPriceDeeplinkQuery, Result<Response<FlightOfferResponse>>>
    {
        private readonly ILogger<OfferPriceDeeplinkQueryHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IDeltaGqlService _deltaGqlService;
        private readonly IDistributedCacheService _distributedCacheService;

        public OfferPriceDeeplinkQueryHandler(IDeltaGqlService deltaGqlService, ILogger<OfferPriceDeeplinkQueryHandler> logger, IMediator mediator, IDistributedCacheService distributedCacheService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
            _deltaGqlService = deltaGqlService ?? throw new ArgumentNullException(nameof(deltaGqlService));
        }

        public async Task<Result<Response<FlightOfferResponse>>> Handle(OfferPriceDeeplinkQuery offerPriceDeeplinkQuery, CancellationToken cancellationToken)
        {
            GqlSearchOffersVariables gqlSearchOffersVariables = new()
            {
                OfferSearchCriteria = OfferMSToDeltaMapper.DeeplinkSummaryRequestMapper(offerPriceDeeplinkQuery)
            };
            var query = File.ReadAllText("GraphqlQuery/gqlOfferPrice.graphql");
            var graphQLResponse = await _deltaGqlService.SendAsync(gqlSearchOffersVariables, query, cancellationToken, $"MetaPartnerId: {offerPriceDeeplinkQuery.Request.MetaPartnerId}");

            if (graphQLResponse.Data == null || graphQLResponse.Data["searchOffers"].HasValues == false)
            {
                return Result.Success(new Response<FlightOfferResponse>
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.DeltaServicesErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, string.Join(", ", graphQLResponse.Errors?.Select(e => e.Message)))
                });
            }

            var response = Json.DeserializeObject<OffersSetList>(graphQLResponse.Data["searchOffers"]);

            if (response.SelectedOfferSets == null || !response.SelectedOfferSets.Any())
            {
                var debugInfo = GetDebugInfo(offerPriceDeeplinkQuery.Request, offerPriceDeeplinkQuery.Request.TotalOfferedAmount.Value, offerPriceDeeplinkQuery.Request.TotalOfferedAmount.Currency);
                _logger.LogError($"No selected offer in OfferPriceQuery. Response: {Json.Serialize(response)}. {debugInfo}");
            }

            var expectedPrice = offerPriceDeeplinkQuery.Request.TotalOfferedAmount;
            var offeredPrice = response.SelectedOfferSets?.First().Offers?.First().OfferPricing?.First().TotalAmt.CurrencyEquivalentPrice;
            if (offeredPrice.CurrencyAmt != expectedPrice.Value || offeredPrice.CurrencyCode != expectedPrice.Currency)
            {
                var debugInfo = GetDebugInfo(offerPriceDeeplinkQuery.Request, offeredPrice?.CurrencyAmt, offeredPrice?.CurrencyCode);
                _logger.LogWarning($"Deeplink price changed from {expectedPrice?.Currency}{expectedPrice?.Value} to {offeredPrice?.CurrencyCode}{offeredPrice?.CurrencyAmt}. {debugInfo}");
            }

            var docId = Guid.NewGuid().ToString();
            var document = new OfferDocument
            {
                Id = docId.ToString(),
                PartitionKey = docId.ToString(),
                CreatedBy = Constants.Application.SearchOfferChannelID,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.IsoDateTimeFormat),
                DocType = Constants.Application.OfferDocType,
                MetaPartnerId = offerPriceDeeplinkQuery.Request.MetaPartnerId,
                FlightOfferResponse = OfferPriceRsMapper.OfferPriceResMapper(offerPriceDeeplinkQuery.Request.Pos, response, docId, isAwardSearch: false, promoCode: null)
            };

            await _distributedCacheService.Set(docId, document, false, TimeSpan.Zero, TimeSpan.FromMinutes(ConfigReader.Try(Constants.Environment.DeltaOfferExpiryLimitInMinutes, 120)));

            return new Response<FlightOfferResponse>
            {
                Status = Status.OK,
                Value = document.FlightOfferResponse
            };
        }

        private static string GetDebugInfo(DeepLinkSummaryRequest request, decimal? price, string currency)
        {
            var segments = string.Join("&segment=", request.FlightSegments.Select(fs => $"{fs.SliceNumber}|{fs.MarketingClassOfService}|{fs.OriginAirportCode}|{fs.DestinationAirportCode}|{fs.MarketingCarrierCode}|{fs.MarketingFlightNumber}|{fs.ScheduledDepartureLocalDateTime}|{fs.FareBasisCode}"));
            var passengerCounts = $"a{request.CustomerDetails.Count(cd => cd.Ptc == "ADT")}t{request.CustomerDetails.Count(cd => cd.Ptc == "GBE")}c{request.CustomerDetails.Count(cd => cd.Ptc == "CNN")}i{request.CustomerDetails.Count(cd => cd.Ptc == "INF")}";
            var queryString = $"?segment={segments}&passengers={passengerCounts}&price={price}&currency={currency}&cabin={request.DominantSegmentBrandId}&vendorid=VS_DEBUG&cm_mmc=0";
            return $"Vendor: {request.MetaPartnerId}. URL with new price: /flight/summary{queryString}";
        }
    }
}