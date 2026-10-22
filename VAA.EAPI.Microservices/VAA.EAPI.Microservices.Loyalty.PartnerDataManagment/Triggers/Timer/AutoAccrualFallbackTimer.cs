using Microsoft.Azure.Functions.Worker;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.Timer
{
    public class AutoAccrualFallbackTimer
    {
        private readonly IAutoAccrualFallbackHandler _handler;

        public AutoAccrualFallbackTimer(IAutoAccrualFallbackHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [Function("AutoAccrualFallbackTimer")]
        public async Task Run([TimerTrigger(Constants.Environment.AutoAccrualFallbackSchedule)] TimerInfo timerInfo, FunctionContext context)
        {
            await _handler.HandleAsync(context.CancellationToken);
        }
    }
}
