using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.Timer
{
    public class ResetPendingRetroTimer
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public ResetPendingRetroTimer(IMediator mediator, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ResetPendingRetroTimer>();
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("AutoRetroInboundResetTimer")]
        public async Task Run([TimerTrigger(Constants.Environment.AutoRetroTimerChrone, RunOnStartup=true)] TimerInfo timerInfo)
        {
            try
            {
                await _mediator.Send(new ResetPendingRetroCommand { Days = ConfigReader.Get(Constants.Environment.AutoRetroResetDuration) }, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AutoRetroInboundResetTimer failing with exception {ex.Message}");
            }
        }
    }
}
