using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Interfaces;

namespace VAA.EAPI.Microservices.Inflight.ConnPulse.Triggers.Timer
{
    public class HealthCheckTimer
    {
        private readonly IInflightConnectivityHealthCheckHandler _inflightConnectivityHealthCheckHandler;
        private readonly ILogger<HealthCheckTimer> _logger;

        public HealthCheckTimer(IInflightConnectivityHealthCheckHandler inflightConnectivityHealthCheckHandler, ILogger<HealthCheckTimer> logger)
        {
            _inflightConnectivityHealthCheckHandler = inflightConnectivityHealthCheckHandler ?? throw new ArgumentNullException(nameof(inflightConnectivityHealthCheckHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function(nameof(HealthCheckTimer))]
        public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo timerInfo, CancellationToken cancellationToken)
        {
            await _inflightConnectivityHealthCheckHandler.HealthCheckAsync(cancellationToken);
        }
    }
}
