using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2
{
    public interface IServiceBusTopicMessageEventCommandHandler : IRequestHandler<ServiceBusTopicMessageEventCommand, Unit>
    {
        public Task<Unit> Handle(ServiceBusTopicMessageEventCommand request, CancellationToken cancellationToken);
    }
}
