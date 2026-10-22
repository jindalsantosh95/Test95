using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Triggers.Timer
{
    public class OTPRecordDeletionTimer
    {
        private readonly IMediator _mediator;

        public OTPRecordDeletionTimer(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("OTPRecordDeletionTimer")]
        public async Task Run([TimerTrigger(Constants.Application.OTPRequestTimer, RunOnStartup = true)] Domain.Timer.TimerInfo timerInfo)
        {
            await _mediator.Send(new OTPRequestTimerCommand());
        }
    }

}
