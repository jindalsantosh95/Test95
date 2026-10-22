using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Timer
{
    public class BirthdayBonusDailyTimer
    {
        private readonly ILogger<BirthdayBonusDailyTimer> _logger;
        private readonly IMediator _mediator;

        public BirthdayBonusDailyTimer(IMediator mediator, ILogger<BirthdayBonusDailyTimer> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("BirthdayBonusDailyTimer")]
        public async Task Run([TimerTrigger("%BirthdayBonusCRON%")] TimerInfo birthdayBonusTimer)
        {
            _logger.LogInformation($"Birthday Bonus Daily Timer trigger function started at: {DateTime.Now}, Last execution: {birthdayBonusTimer?.ScheduleStatus?.Last}, Current execution: {birthdayBonusTimer?.ScheduleStatus?.Next}");
            await _mediator.Send(new BirthdayBonusCommand() { Id = Guid.NewGuid() });
            _logger.LogInformation($"Birthday Bonus Daily Timer trigger function end at: {DateTime.Now}");
        }
    }
}
