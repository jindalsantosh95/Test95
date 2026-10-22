using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Operations.Domain.AirportInfo;
using VAA.EAPI.Microservices.Operations.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Operations.Application.Query
{
    public class AirportInfoQuery : IRequest<Result<Response<ICollection<AirportInfo>>>>
    {
    }

    public class AirportInfoQueryHandler : IRequestHandler<AirportInfoQuery, Result<Response<ICollection<AirportInfo>>>>
    {
        private readonly IAirportInfoProvider _airportInfoProvider;
        public AirportInfoQueryHandler(IAirportInfoProvider airportInfoProvider, ILogger<AirportInfoQueryHandler> logger)
        {
            _airportInfoProvider = airportInfoProvider ?? throw new System.ArgumentNullException(nameof(airportInfoProvider));
            _ = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<ICollection<AirportInfo>>>> Handle(AirportInfoQuery request, CancellationToken cancellationToken)
        {
            var airportList = await _airportInfoProvider.GetAllAirportsAsync();
            return new Response<ICollection<AirportInfo>>
            {
                Status = Status.OK,
                Value = airportList
            };
        }
    }
}
