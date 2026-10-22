using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Mapper.Airportdetails;
using VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb.Interface;

namespace VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Repositories.AirportDetailsRepo
{
    public class AirportDetailsRepoCommandHandler : IRequestHandler<AirportDetailsRepoCommand, Result<Response<List<Domain.Airportdetails.Model.Airportdetail>>>>
    {
        private readonly ICommonRefDBService _commonDBService;
        private readonly ILogger<AirportDetailsRepoCommandHandler> _logger;

        public AirportDetailsRepoCommandHandler(ICommonRefDBService commonDBService, ILogger<AirportDetailsRepoCommandHandler> logger)
        {
            _commonDBService = commonDBService ?? throw new ArgumentNullException(nameof(commonDBService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<List<Domain.Airportdetails.Model.Airportdetail>>>> Handle(AirportDetailsRepoCommand request, CancellationToken cancellationToken)
        {
            Response<List<Domain.Airportdetails.Model.Airportdetail>> airportDetailsQueryResponse = new Response<List<Domain.Airportdetails.Model.Airportdetail>>();
            var airportDetails = await _commonDBService.GetAllAirportsAsync();
            airportDetailsQueryResponse.Value = AirportDataMapper.MapAirportData(airportDetails);
            return airportDetailsQueryResponse;

        }
    }
}
