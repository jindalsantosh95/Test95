using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.ServiceBus
{
    public class CPDPaymentSubscriber
    {
        private readonly IMediator _mediator;

        public CPDPaymentSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("RpaNdthMessageSubscriber")]
        public async Task RpaNdth([ServiceBusTrigger(Constants.Application.NtdhTopicName, Constants.Application.RpaNtdhSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var cloudEvent = Json.Deserialize<RpaNdthEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }

    }
}
