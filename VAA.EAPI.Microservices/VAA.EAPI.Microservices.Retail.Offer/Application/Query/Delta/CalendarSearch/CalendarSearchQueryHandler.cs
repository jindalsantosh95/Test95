using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Graphql;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain.CalendarSearch;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta.CalendarSearch;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.CalendarSearch
{
    public class CalendarSearchQueryHandler : IRequestHandler<CalendarSearchQuery, Result<Response<CalendarSearchResponse>>>
    {
        private readonly IDeltaGqlService _deltaGqlService;
        private readonly ILogger<CalendarSearchQueryHandler> _logger;

        public CalendarSearchQueryHandler(IDeltaGqlService deltaGqlService, ILogger<CalendarSearchQueryHandler> logger)
        {
            _deltaGqlService = deltaGqlService ?? throw new ArgumentNullException(nameof(deltaGqlService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<CalendarSearchResponse>>> Handle(CalendarSearchQuery request, CancellationToken cancellationToken)
        {
            var query = File.ReadAllText("GraphqlQuery/gqlCalendarSearch.graphql");

            GqlSearchOffersVariables gqlSearchOffersVariables = new()
            {
                OfferSearchCriteria = request.Request
            };

            var response = await _deltaGqlService.SendAsync(gqlSearchOffersVariables, query, cancellationToken);
            if (!response.Errors.Any())
            {
                var offersSetList = Json.DeserializeObject<OffersSetList>(response.Data["searchOffers"]);
                return new Response<CalendarSearchResponse>
                {
                    Status = Status.OK,
                    Value = CalendarSearchRSMapper.GetCalendar(request.AwardSeach, offersSetList)
                };
            }
            else
            {
                return Result.Success(new Response<CalendarSearchResponse>
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.DeltaServicesErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, string.Join(", ", response.Errors?.Select(e => e.Message)))
                });
            }
        }
    }
}
