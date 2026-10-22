using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Flight.Services.Domain;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Flight.Services.Application.Query
{
    public class FlightServiceOfferSearchQueryHandler : IRequestHandler<FlightServiceOfferSearchQuery, Result<Response<FlightServiceResponse>>>
    {

        private readonly IMediator _mediator;

        public FlightServiceOfferSearchQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<FlightServiceResponse>>> Handle(FlightServiceOfferSearchQuery request, CancellationToken cancellationToken)
        {
            var flightServiceOfferResponse = await _mediator.Send(new FlightServiceOfferSearchRepoQuery(request.FlightServiceOfferID), cancellationToken);

            if (flightServiceOfferResponse?.Offers == null)
            {
                return Result.Success
                    (
                        new Response<FlightServiceResponse>
                        {
                            Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.InvalidOfferRefId, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                            Status = Status.BusinessException
                        }
                    );
            }

            return Result.Success(new Response<FlightServiceResponse>
            {
                Status = Status.OK,
                Value = flightServiceOfferResponse
            });
        }
    }
}
