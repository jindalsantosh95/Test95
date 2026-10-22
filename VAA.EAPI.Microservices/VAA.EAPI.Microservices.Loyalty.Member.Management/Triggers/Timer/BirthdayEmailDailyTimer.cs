using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Timer
{
    public class BirthdayEmailDailyTimer
    {
        private readonly ILogger<BirthdayEmailDailyTimer> _logger;
        private readonly IMediator _mediator;
        public BirthdayEmailDailyTimer(IMediator mediator, ILogger<BirthdayEmailDailyTimer> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("BirthdayEmailDailyTimer")]
        public async Task Run([TimerTrigger("%BirthdayEmailCRON%")] TimerInfo birthdayEmailTimer)
        {
            _logger.LogInformation($"Birthday Email Daily Timer trigger function started at: {DateTime.Now}, Last execution: {birthdayEmailTimer?.ScheduleStatus?.Last}, Current execution: {birthdayEmailTimer?.ScheduleStatus?.Next}");
            await _mediator.Send(new BirthdayEmailCommand() { Id = Guid.NewGuid() });
            _logger.LogInformation($"Birthday Email Daily Timer trigger function end at: {DateTime.Now}");
        }
    }

}
