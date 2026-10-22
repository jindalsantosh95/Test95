using System.Collections.Concurrent;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;

namespace VAA.EAPI.Core.Xunit.ServiceBus
{
    public sealed class MessageBusFactory : IMessageBusFactory
    {
        private readonly ConcurrentDictionary<string, IMessageBus> _clients = [];

        public IMessageBus GetClient(string connectionString, string sender)
        {
            if (connectionString.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(connectionString));
            if (sender.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(sender));

            return _clients.GetOrAdd($"{connectionString}:{sender}", new MessageBus());
        }
    }
}
