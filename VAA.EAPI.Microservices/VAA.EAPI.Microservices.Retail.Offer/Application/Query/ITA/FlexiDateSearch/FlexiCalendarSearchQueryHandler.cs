using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Validation.Ita;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.FlexiDateSearch
{
    public class FlexiCalendarSearchQueryHandler : IRequestHandler<FlexiCalendarSearchQuery, Result<Response<FlexiDateSearchResponse>>>
    {
        private readonly IMediator _mediator;

        public FlexiCalendarSearchQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<FlexiDateSearchResponse>>> Handle(FlexiCalendarSearchQuery requestData, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new ItaRepoQuery { Search = FlexiDateItaReqMapper.CalendarSearchReqMapper(requestData, true) }, cancellationToken);
            if (response.messages?.error != null || response.error != null)
            {
                var results = GetITAErrorsFromResponse.GetITAErrors(response);
                if (results.Value.Status != Status.OK)
                {
                    Status status = results.Value.Errors[0].DeveloperMessage.Contains(Constants.Application.ITAError) ? Status.Error : results.Value.Status;
                    return Result.Success(new Response<FlexiDateSearchResponse>
                    {
                        Errors = results.Value.Errors,
                        Status = status
                    });
                }
            }
            return new Response<FlexiDateSearchResponse>
            {
                Status = Status.OK,
                Value = FlexiDateItaResMapper.FlexiDateSearchResMapper(response, requestData)
            };
        }
    }
}
