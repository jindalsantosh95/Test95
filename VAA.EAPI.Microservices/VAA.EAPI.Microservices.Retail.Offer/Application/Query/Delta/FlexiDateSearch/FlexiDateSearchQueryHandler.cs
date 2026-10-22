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
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta.FlexiDateSearch;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlexiDateSearch
{
    public class FlexiDateSearchQueryHandler : IRequestHandler<FlexiDateSearchQuery, Result<Response<FlexiDateSearchResponse>>>
    {
        private readonly IDeltaGqlService _deltaGqlService;
        private readonly ILogger<FlexiDateSearchQueryHandler> _logger;

        public FlexiDateSearchQueryHandler(IDeltaGqlService deltaGqlService, ILogger<FlexiDateSearchQueryHandler> logger)
        {
            _deltaGqlService = deltaGqlService ?? throw new ArgumentNullException(nameof(deltaGqlService));
            _logger = logger ?? throw new ArgumentNullException();
        }

        public async Task<Result<Response<FlexiDateSearchResponse>>> Handle(FlexiDateSearchQuery request, CancellationToken cancellationToken)
        {
            var query = File.ReadAllText("GraphqlQuery/gqlFlexiDateSearch.graphql");

            GqlSearchOffersVariables gqlSearchOffersVariables = new()
            {
                OfferSearchCriteria = request.Request
            };

            var response = await _deltaGqlService.SendAsync(gqlSearchOffersVariables, query, cancellationToken);

            if (!response.Errors.Any())
            {
                var offersSetList = Json.DeserializeObject<OffersSetList>(response.Data["searchOffers"]);
                return new Response<FlexiDateSearchResponse>
                {
                    Status = Status.OK,
                    Value = FlexiSearchResMapper.FlexiDateSearchResMapper(offersSetList, request)
                };
            }
            else
            {
                return Result.Success(new Response<FlexiDateSearchResponse>
                {
                    Status = Status.BusinessException,
                    Errors = ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.DeltaServicesErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, string.Join(", ", response.Errors?.Select(e => e.Message)))
                });
            }
        }
    }
}
