using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Command.PartnerReferenceData;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Triggers.Timer
{
    public class PartnerReferenceDataTimer
    {
        private readonly IPartnerReferenceDataCommandHandler _handler;

        public PartnerReferenceDataTimer(IPartnerReferenceDataCommandHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [Function("PartnerReferenceDataTimer")]
        public async Task Run([TimerTrigger("%PartnerDetailsPublishSchedule%")] TimerInfo timerInfo, FunctionContext context)
        {
            await _handler.Handle(new PartnerReferenceDataCommand(), context.CancellationToken);
        }
    }
}

