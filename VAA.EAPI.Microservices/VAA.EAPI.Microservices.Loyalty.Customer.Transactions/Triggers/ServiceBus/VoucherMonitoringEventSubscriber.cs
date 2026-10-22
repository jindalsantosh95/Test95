using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.VoucherMonitoring;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class VoucherMonitoringEventSubscriber
    {
        private readonly IVoucherMonitoringHandler _handler;

        public VoucherMonitoringEventSubscriber(IVoucherMonitoringHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [Function("VoucherUsageProactiveMonitoringEventSubscriber")]
        public async Task MonitorVoucherRedemptions([ServiceBusTrigger(Constants.Application.TransactionEventTopicName, Constants.Application.VoucherMonitoringSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            await _handler.Handle(Json.Deserialize<VoucherMonitoringEvent>(eventRequestPayload), cancellationToken);
        }
    }
}
