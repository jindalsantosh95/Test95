using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderStore;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus
{
    public class OrderInsuranceStatusUpdateSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public OrderInsuranceStatusUpdateSubscriber(IMediator mediator, ILogger<OrderCreateMessageSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("OrderInsuranceStatusUpdateSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Environment.InsurancePurchaseStatusUpdateTopicName, Constants.Environment.InsurancePurchaseStatusSubscriptionName, Connection = "OrderServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var insuranceUpdateCommand = Json.Deserialize<InsuranceStatusUpdateCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(insuranceUpdateCommand);

            await _mediator.Send(insuranceUpdateCommand);
        }
    }
}
