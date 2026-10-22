using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.IFG;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Triggers.ServiceBus
{
    public class CreatePaymentDocumentSubscriber
    {
        private readonly ICreatePaymentDocumentEvent _createPaymentDocumentEvent;
        private readonly CancellationToken _cancellationToken;

        public CreatePaymentDocumentSubscriber(ICreatePaymentDocumentEvent createPaymentDocumentEvent)
        {
            _createPaymentDocumentEvent = createPaymentDocumentEvent ?? throw new ArgumentNullException(nameof(createPaymentDocumentEvent));
            _cancellationToken = new CancellationToken();
        }

        [Function("CreatePaymentDocumentSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CreatePaymentDocumentTopicName, Constants.Application.CreatePaymentDocumentSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var eventRequest = Json.Deserialize<CloudEvent>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(eventRequest);

            await _createPaymentDocumentEvent.Handle(eventRequest, _cancellationToken);
        }
    }
}
