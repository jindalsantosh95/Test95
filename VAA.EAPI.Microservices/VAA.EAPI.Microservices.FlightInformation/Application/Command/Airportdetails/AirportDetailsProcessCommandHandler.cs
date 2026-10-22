using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightInformation.Application.Query.Airportdetails;

namespace VAA.EAPI.Microservices.FlightInformation.Application.Command.Airportdetails
{
    public class AirportDetailsProcessCommandHandler : IRequestHandler<AirportDetailsProcessCommand, Result<Response<string>>>
    {
        private readonly ILogger<AirportDetailsProcessCommandHandler> _logger;
        private readonly IMediator _mediator;
        public AirportDetailsProcessCommandHandler(IMediator mediator, ILogger<AirportDetailsProcessCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(AirportDetailsProcessCommand request, CancellationToken cancellationToken)
        {
            Result<Response<string>> response;
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                var airportDetails = await _mediator.Send(new AirportDetailsQuery());
                if (airportDetails.IsSuccess && airportDetails.Value?.Value.Count > 0)
                {
                    var mergedResult = await _mediator.Send(new AirportDetailsMergeCommand() { AirportDetails = airportDetails.Value?.Value });

                    if (mergedResult.Count > 0)
                    {
                        var airportDetailsMergeEventCommand = new AirportDetailsMergeEventCommand()
                        {
                            PayLoad = mergedResult

                        };
                        response = await _mediator.Send(airportDetailsMergeEventCommand);
                    }
                    else
                    {
                        response = Result.Success<Response<string>>(new Response<string>() { Value = Constants.ErrorCodes.NoRecordToMerge });
                    }

                    return response.Value != null ? Result.Success<Response<string>>(response.Value) : Result.Failure<Response<string>>(Constants.ErrorCodes.ErrorWhileProcessingMerging);
                }
                else
                {
                    return Result.Failure<Response<string>>(airportDetails.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AirportDetailsProcessCommandHandler.Handle failed.");
                return Result.Failure<Response<string>>(Convert.ToString(ex.Message));
            }
        }
    }
}
