using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.CRM.Application.Commands;

namespace VAA.EAPI.Microservices.CRM.Triggers.ServiceBus
{
    public class UpdatePaymentMessageSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdatePaymentMessageSubscriber> _logger;

        public UpdatePaymentMessageSubscriber(IMediator mediator, ILogger<UpdatePaymentMessageSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("CRMPaymentUpdateMessageSubscriber")]
        public async Task Run([ServiceBusTrigger("%CRMPaymentEventHandlerTopicName%", "%CRMPaymentEventSubscriptionName%", Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            if (ConfigReader.Get<bool>(Constants.Environment.CRMWebApiResponsiveFeature))
            {
                var cloudEvent = Json.Deserialize<PaymentUpdateEventProcessCommandV2>(eventRequestPayload);
                VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
                _logger.LogInformation($"Correlation ID {cloudEvent.Id}");
                await _mediator.Send(cloudEvent, CancellationToken.None);
            }
            else
            {
                var cloudEvent = Json.Deserialize<PaymentUpdateEventProcessCommand>(eventRequestPayload);
                VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
                _logger.LogInformation($"Correlation ID {cloudEvent.Id}");
                await _mediator.Send(cloudEvent, CancellationToken.None);
            }
        }
    }
}
