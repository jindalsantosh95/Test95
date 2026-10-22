using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.WingsMemberAssessment;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Timer
{
    public class WingsMemberAssessmentTimer
    {
        private readonly IWingsMemberAssessmentCommand _commandHandler;
        private readonly ILogger<WingsMemberAssessmentTimer> _logger;

        public WingsMemberAssessmentTimer(
            IWingsMemberAssessmentCommand commandHandler,
            ILogger<WingsMemberAssessmentTimer> logger)
        {
            _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function(nameof(WingsMemberAssessmentTimer))]
        public async Task Run(
            [TimerTrigger(Constants.Environment.WingsMemberAssessmentTimerSchedule)] TimerInfo timerInfo,
            CancellationToken cancellationToken)
        {
            await _commandHandler.Handle(
                new WingsMemberAssessmentCommand { Id = Guid.NewGuid() },
                cancellationToken);
        }
    }
}
