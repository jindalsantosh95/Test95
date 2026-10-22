using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta;
using System.IO;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightOfferGql
{
    public class FlightOfferGqlServiceQueryHandler : IRequestHandler<FlightOfferGqlServiceQuery, Response<FlightOfferResponse>>
    {
        private readonly IDeltaGqlService _deltaGqlService;
        private readonly ILogger<FlightOfferServiceQueryHandler> _logger;

        public FlightOfferGqlServiceQueryHandler(IDeltaGqlService deltaGqlService, ILogger<FlightOfferServiceQueryHandler> logger)
        {
            _deltaGqlService = deltaGqlService ?? throw new ArgumentNullException(nameof(deltaGqlService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<FlightOfferResponse>> Handle(FlightOfferGqlServiceQuery request, CancellationToken cancellationToken)
        {
            int resultsPageNum = 0;
            int resultsPerRequestNum = 0;
            if (ConfigReader.Get<bool>(Constants.Environment.DlOfferPagingEnable))
            {
                resultsPageNum = ConfigReader.Get<int>(Constants.Environment.DlResultPageNumber);
                resultsPerRequestNum = ConfigReader.Get<int>(Constants.Environment.DlResultCountPerPage);
            }

            var variables = FlightOfferMapper.FlightOfferRequestMapper(request.Request, resultsPageNum, resultsPerRequestNum);
            var query = request.Request.FlightsearchRequest.AwardSearch == false
                ? File.ReadAllText($"GraphqlQuery/gqlSearchOffers.graphql")
                : File.ReadAllText($"GraphqlQuery/gqlAwardSearchOffers.graphql");

            var graphQlResponse = await _deltaGqlService.SendAsync(new GqlSearchOffersVariables { OfferSearchCriteria = variables }, query, cancellationToken);

            var searchLog = new Dictionary<string, object>
            {
                { "numResults", 0 },
                { "isAward", request.Request.FlightsearchRequest.AwardSearch },
                { "numSlices", request.Request.FlightsearchRequest.SearchOriginDestinations.Count() },
                { "isFirstSlice", request.Request.FlightsearchRequest.OfferID == null },
                { "slices", request.Request.FlightsearchRequest.SearchOriginDestinations.Select(od => new Dictionary<string, object>
                    {
                        { "origin", od.Origin },
                        { "destination", od.Destination },
                        { "departure", od.DepartureDate }
                    }
                )},
                { "pax", request.Request.CustomerDetails.GroupBy(cd => cd.Ptc).Select(g => g).OrderBy(g => g.Key.ToUpper()).ToDictionary(g => g.Key.ToUpper(), g => g.Count()) }
            };

            if (!graphQlResponse.Errors.Any())
            {
                var searchResults = Json.DeserializeObject<OffersSetList>(graphQlResponse.Data["searchOffers"]);
                var flightOfferResponse = FlightOfferMapper.FlightOfferResponseMapper(request.Request, searchResults, request.DocId.ToString(), request.SliceZeroPrice, _logger);

                searchLog["numResults"] = searchResults.OffersSets.Count();
                _logger.LogInformation($"Correlation id:{AsyncContext.Current.CorrelationId} | SearchOffers completed: {Json.Serialize(searchLog)}");

                return new Response<FlightOfferResponse>
                {
                    Status = Status.OK,
                    Value = flightOfferResponse
                };
            }
            else
            {
                var errorResponse = graphQlResponse.Errors.First().Extensions.Errors;
                if (errorResponse?.MoreInfo?.Any() == true)
                {
                    searchLog.Add("errorCode", errorResponse.MoreInfo.First().Code);
                    searchLog.Add("error", errorResponse.MoreInfo.First().Message);
                }
                _logger.LogInformation($"Correlation id:{AsyncContext.Current.CorrelationId} | SearchOffers completed: {Json.Serialize(searchLog)}");

                var innerErrorResponse = errorResponse.MoreInfo.FirstOrDefault();
                var correlationId = AsyncContext.Current.CorrelationId;

                // ITA error codes are three digit numbers. Pass these through to the consuming service.
                if (innerErrorResponse?.Code.Length == 3 && innerErrorResponse.Code.All(char.IsDigit))
                {
                    return new Response<FlightOfferResponse>
                    {
                        Status = Status.BusinessException,
                        Errors =
                        [
                            new ErrorDetails
                            {
                                Code = innerErrorResponse.Code,
                                Message = innerErrorResponse.Message,
                                DeveloperMessage = $"{errorResponse.DeveloperMessage} | Correlation ID: {correlationId} | TimeStamp: {DateTime.UtcNow}",
                                ErrorSource = "Delta"
                            }
                        ]
                    };
                }
                else
                {
                    return new Response<FlightOfferResponse>
                    {
                        Status = Status.BusinessException,
                        Errors = ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.DeltaServicesErrorCode, correlationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, errorResponse.Message, $"{errorResponse.DeveloperMessage} | {Json.Serialize(errorResponse.MoreInfo)}")
                    };
                }
            }
        }
    }
}
