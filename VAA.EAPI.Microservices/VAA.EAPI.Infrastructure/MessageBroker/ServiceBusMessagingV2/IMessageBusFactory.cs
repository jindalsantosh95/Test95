namespace VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2
{
    public interface IMessageBusFactory
    {
        IMessageBus GetClient(string connectionString, string sender);
    }

}
