using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.InsurancePurchase;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.ServiceBus
{
    public class InsurancePurchaseMessageSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InsurancePurchaseMessageSubscriber> _logger;

        public InsurancePurchaseMessageSubscriber(IMediator mediator, ILogger<InsurancePurchaseMessageSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("InsurancePurchaseMessageSubscriber")]
        public async Task InsurancePurchase([ServiceBusTrigger(Constants.Application.OrderInsurancePurchaseMessage, Constants.Application.OrderInsuranceEventSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var insurancePurchaseEventCommand = Json.Deserialize<InsurancePurchaseEventCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(insurancePurchaseEventCommand);

            await _mediator.Send(insurancePurchaseEventCommand, CancellationToken.None);
        }
    }
}
