using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightOffer
{
    public class FlightOfferServiceQueryHandler : IRequestHandler<FlightOfferServiceQuery, Response<FlightOfferResponse>>
    {
        private readonly ILogger<FlightOfferServiceQueryHandler> _logger;
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;

        public FlightOfferServiceQueryHandler(ILogger<FlightOfferServiceQueryHandler> logger, IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
        }

        public async Task<Response<FlightOfferResponse>> Handle(FlightOfferServiceQuery request, CancellationToken cancellationToken)
        {
            bool isReturnTripSliceZero = request.Request.FlightsearchRequest.SearchOriginDestinations.Count > 1 && request.Request.FlightsearchRequest.CurrentTripIndexId == Constants.Application.SliceZeroIndex;

            try
            {
                var SwitchToAws = ConfigReader.Get<bool>(Constants.Environment.SwitchToAws);
                var offerUri = SwitchToAws ? ConfigReader.Get(Constants.Environment.DlAwsOfferUri) : ConfigReader.Get(Constants.Environment.DlOfferUri);
                var token = SwitchToAws
                    ? await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlAwsOfferClientId), ConfigReader.Get(Constants.Environment.DlAwsOfferClientSecret), SwitchToAws)
                    : await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlOfferClientId), ConfigReader.Get(Constants.Environment.DlOfferClientSecret), SwitchToAws);

                OffersSetList offerRs = new();
                int resultsPageNum = 0;
                int resultsPerRequestNum = 0;
                if (ConfigReader.Get<bool>(Constants.Environment.DlOfferPagingEnable))
                {
                    resultsPageNum = ConfigReader.Get<int>(Constants.Environment.DlResultPageNumber);
                    resultsPerRequestNum = ConfigReader.Get<int>(Constants.Environment.DlResultCountPerPage);
                }

                var dlOfferRequest = FlightOfferMapper.FlightOfferRequestMapper(request.Request, resultsPageNum, resultsPerRequestNum);
                _logger.LogInformation($"CorrelationId: {AsyncContext.Current.CorrelationId}, Delta OfferApi Criteria \n {dlOfferRequest}");

                var headers = new Dictionary<string, string>
                    {
                         {"applicationId", ConfigReader.Get(Constants.Environment.DlApiApplicationId)},
                         {"channelId", ConfigReader.Get(Constants.Environment.DlApiChannelId)},
                         {"transactionId", AsyncContext.Current.CorrelationId.ToString()}
                    };

                _logger.LogInformation($"CorrelationId: {AsyncContext.Current.CorrelationId}, Delta request \n {Json.Serialize(dlOfferRequest)}");

                offerRs = await _apiConnector.SendAsync<OffersSetList>(offerUri, headers: headers, bearerToken: token, verb: HttpMethod.Post, content: Json.Serialize(dlOfferRequest));

                var flightOfferResponse = FlightOfferMapper.FlightOfferResponseMapper(request.Request, offerRs, request.DocId.ToString(), request.SliceZeroPrice, _logger);

                return new Response<FlightOfferResponse>
                {
                    Status = Status.OK,
                    Value = flightOfferResponse
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Correlation id:{AsyncContext.Current.CorrelationId} Request Processing Terminated for DeltaOfferService");

                if (ex.InnerException is HttpResponseException httpResponseException)
                {
                    var errorResponse = Json.Deserialize<DeltaErrorResponse>(await httpResponseException.Response.Content.ReadAsStringAsync(CancellationToken.None));
                    if (isReturnTripSliceZero && errorResponse.MoreInfo.Any(x => x.Code == ErrorCodes.NoOffersDeltaError || x.Code == ErrorCodes.NoOffersSecondSliceDeltaError))
                    {
                        return new Response<FlightOfferResponse>
                        {
                            Status = Status.BusinessException,
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.OfferSearchNoOffers), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                        };
                    }
                    return new Response<FlightOfferResponse>
                    {
                        Status = Status.BusinessException,
                        Errors = ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.DeltaServicesErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, Json.Serialize(errorResponse))
                    };
                }
                throw;
            }
        }
    }
}
