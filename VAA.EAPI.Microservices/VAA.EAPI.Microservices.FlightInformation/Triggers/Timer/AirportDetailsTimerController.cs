using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.FlightInformation.Application.Command.Airportdetails;

namespace VAA.EAPI.Microservices.FlightInformation.Triggers.Timer
{
    public class AirportDetailsTimerController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AirportDetailsTimerController> _logger;
        public AirportDetailsTimerController(IMediator mediator, ILogger<AirportDetailsTimerController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [Function("AirportDetailsTimer")]
        public async Task Run([TimerTrigger(Constants.Application.AirportDetailsLookUpTimer, RunOnStartup = true)] TimerInfo timerInfo, FunctionContext context)
        {
            var timerResult = await _mediator.Send(new AirportDetailsProcessCommand());
            if (!timerResult.IsSuccess)
            {
                _logger.LogError("AirportDetailsTimer returned an unsuccessful result. Error: {Error}", timerResult.Error);
            }
        }
    }
}
