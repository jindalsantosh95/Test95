using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Repositories.AirportDetailsRepo;

namespace VAA.EAPI.Microservices.FlightInformation.Application.Query.Airportdetails
{

    public class AirportDetailsQueryHandler : IRequestHandler<AirportDetailsQuery, Result<Response<List<Domain.Airportdetails.Model.Airportdetail>>>>
    {
        private readonly IMediator _mediator;
        public AirportDetailsQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<Result<Response<List<Domain.Airportdetails.Model.Airportdetail>>>> Handle(AirportDetailsQuery request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return await _mediator.Send(new AirportDetailsRepoCommand());
        }
    }
}
