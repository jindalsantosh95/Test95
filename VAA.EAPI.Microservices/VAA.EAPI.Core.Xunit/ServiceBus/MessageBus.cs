using System.Collections.Concurrent;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;

namespace VAA.EAPI.Core.Xunit.ServiceBus
{
    public sealed class MessageBus : IMessageBus
    {
        private readonly ConcurrentQueue<CloudEvent> _items = [];

        public event Action<CloudEvent> OnMessage;

        public int Delivered { get; private set; }
        public int Count => _items.Count;

        public async Task PublishMessageAsync(CloudEvent @event)
        {
            Common.AsyncContext.Current?.MapToCloudEvent(@event);

            _items.Enqueue(@event);

            await Task.Delay(100);

            if (_items.TryDequeue(out var item))
            {
                Delivered++;
                OnMessage?.Invoke(item);
            }
        }
    }
}
