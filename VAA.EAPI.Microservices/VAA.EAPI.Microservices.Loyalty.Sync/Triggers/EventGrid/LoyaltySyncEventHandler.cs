using Azure.Messaging.EventGrid;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Triggers.EventGrid
{
    public class LoyaltySyncEventHandler
    {
        private readonly IMediator _mediator;

        public LoyaltySyncEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("LoyaltySyncEvent")]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent, FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("HttpFunction");
            log.LogInformation("Loyalty Sync Event received");
            await _mediator.Send(Json.Deserialize<LoyaltySyncEventProcessCommand>(Convert.ToString(eventGridEvent.Data)));
        }
    }
}
