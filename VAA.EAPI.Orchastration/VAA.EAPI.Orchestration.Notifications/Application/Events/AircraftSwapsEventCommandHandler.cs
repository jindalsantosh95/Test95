using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces;

namespace VAA.EAPI.Orchestration.Notifications.Application.Events
{
    public sealed class AircraftSwapsEventCommandHandler(ILogger<AircraftSwapsEventCommandHandler> logger, IAircraftSwapsDelayedDispatcherService delayedDispatcherService) : IAircraftSwapsEventCommand
    {
        private readonly ILogger<AircraftSwapsEventCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAircraftSwapsDelayedDispatcherService _delayedDispatcherService = delayedDispatcherService ?? throw new ArgumentNullException(nameof(delayedDispatcherService));

        public async Task Handle(CloudEvent cloudEventReq, CancellationToken cancellationToken)
        {
            try
            {
                var request = (AircraftMessage)Convert.ToString(cloudEventReq.Data);

                if (request is null || request.Category != "EQT" || string.IsNullOrWhiteSpace(request.FlightNumber) || request.FlightDate == default || request.FlightDate <= DateTime.Today)
                {
                    return;
                }

                var delay = TimeSpan.Parse(ConfigReader.Get(Constants.Environment.AircraftChangeNotifDelay));
                _delayedDispatcherService.Schedule(request, delay);
            }
            catch (FormatException fex)
            {
                _logger.LogError(fex, "Failed to parse AircraftSwaps message due to a format exception. Payload: {Payload}", cloudEventReq.Data?.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback event from Event store due to error in AircraftSwapsEventCommandHandler: {ex.Message}");
            }
        }

    }

}

