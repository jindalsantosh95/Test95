using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.ServiceBus
{
    public class ClearPaymentDetailsSubscriber
    {
        private readonly IMediator _mediator;

        public ClearPaymentDetailsSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("ClearPaymentDetailsSubscriber")]
        public async Task ClearPaymentDetails([ServiceBusTrigger(Constants.Application.ClearPaymentDetailsMessage, Constants.Application.ClearPaymentDetailsSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var clearCVVEventCommand = Json.Deserialize<ClearPaymentDetailsEventCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(clearCVVEventCommand);

            await _mediator.Send(clearCVVEventCommand, CancellationToken.None);
        }
    }
}