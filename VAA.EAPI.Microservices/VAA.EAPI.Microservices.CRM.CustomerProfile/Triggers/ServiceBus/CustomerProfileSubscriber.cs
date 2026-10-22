using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Application.Events;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Triggers.ServiceBus
{
    public class CustomerProfileSubscriber
    {
        private readonly ICustomerProfileEventCommand _customerProfileEventCommand;

        public CustomerProfileSubscriber(ICustomerProfileEventCommand customerProfileEventCommand)
        {
            _customerProfileEventCommand = customerProfileEventCommand ?? throw new ArgumentNullException(nameof(customerProfileEventCommand));
        }

        [Function("CustomerProfileSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Constants.CheckinNotificationTopicName, Constants.Constants.CustomerProfileEventSubscriptionName, Connection = Constants.Environment.OrderServiceBusConnectionString)] string eventRequestPayload, CancellationToken cancellationToken)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);

            AsyncContext.Current.SetDiagnosticsEnabled(cloudEvent.DiagnosticsEnabled);
            AsyncContext.Current.SetSessionId(cloudEvent.SessionId);
            AsyncContext.Current.SetOperationId(cloudEvent.OperationId);
            AsyncContext.Current.SetCorrelationId(cloudEvent.CorrelationId);
            AsyncContext.Current.SetChannel(cloudEvent.Channel);

            await _customerProfileEventCommand.Handle(cloudEvent, cancellationToken);
        }
    }
}
