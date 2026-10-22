using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Triggers.ServiceBus
{
    public class ManageCustomerMessageSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ManageCustomerMessageSubscriber> _logger;
        public ManageCustomerMessageSubscriber(IMediator mediator, ILogger<ManageCustomerMessageSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("ManageCustomerMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.ManageCustomerEventHandlerTopicName, Constants.Application.ManageCustomerEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("ManageCustomerMessageSubscriber event received");

            var cloudEvent = Json.Deserialize<ManageCustomerEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
