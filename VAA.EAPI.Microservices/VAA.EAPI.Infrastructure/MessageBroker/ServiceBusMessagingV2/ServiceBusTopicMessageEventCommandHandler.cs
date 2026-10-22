using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2
{
    public class ServiceBusTopicMessageEventCommandHandler(IMessageBusFactory messageBusFactory) : IServiceBusTopicMessageEventCommandHandler
    {
        private readonly IMessageBusFactory _messageBusFactory = messageBusFactory ?? throw new ArgumentNullException(nameof(messageBusFactory));

        public async Task<Unit> Handle(ServiceBusTopicMessageEventCommand request, CancellationToken cancellationToken)
        {
            var client = this._messageBusFactory.GetClient(request.ServiceBusConnectionString, request.ServiceBusTopicName);

            await client.PublishMessageAsync(request.Message);

            return Unit.Value;
        }
    }
}
