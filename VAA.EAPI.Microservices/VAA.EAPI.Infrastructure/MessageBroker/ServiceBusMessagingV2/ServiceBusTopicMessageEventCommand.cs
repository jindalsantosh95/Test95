using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2
{
    public class ServiceBusTopicMessageEventCommand : IRequest<Unit>
    {
        public string ServiceBusConnectionString { get; set; }
        public string ServiceBusTopicName { get; set; }
        public CloudEvent Message { get; set; }
    }
}
