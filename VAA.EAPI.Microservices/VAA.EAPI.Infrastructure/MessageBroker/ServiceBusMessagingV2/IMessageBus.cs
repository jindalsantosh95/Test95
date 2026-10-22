using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2
{
    public interface IMessageBus : IMessageBus<CloudEvent>
    {
    }

    public interface IMessageBus<T>
    {
        Task PublishMessageAsync(T @event);
    }
}
