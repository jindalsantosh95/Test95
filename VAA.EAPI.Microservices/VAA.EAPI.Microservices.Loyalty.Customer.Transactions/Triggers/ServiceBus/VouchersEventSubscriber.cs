using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Vouchers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class VouchersEventSubscriber
    {
        private readonly IRedeemVoucherEventHandler _redeemVoucherEventHandler;
        private readonly IRollbackVoucherRdmEventHandler _rollbackVoucherRdmEventHandler;
        private readonly IPatchVoucherRelatedTxnsPnrEventHandler _patchVoucherRelatedTxnsPnrEventHandler;

        public VouchersEventSubscriber(IRedeemVoucherEventHandler redeemVoucherEventHandler, IRollbackVoucherRdmEventHandler rollbackVoucherRdmEventHandler, IPatchVoucherRelatedTxnsPnrEventHandler patchVoucherRelatedTxnsPnrEventHandler)
        {
            _redeemVoucherEventHandler = redeemVoucherEventHandler ?? throw new ArgumentNullException(nameof(redeemVoucherEventHandler));
            _rollbackVoucherRdmEventHandler = rollbackVoucherRdmEventHandler ?? throw new ArgumentNullException(nameof(rollbackVoucherRdmEventHandler));
            _patchVoucherRelatedTxnsPnrEventHandler = patchVoucherRelatedTxnsPnrEventHandler ?? throw new ArgumentNullException(nameof(patchVoucherRelatedTxnsPnrEventHandler));
        }

        [Function("RedeemVoucherEventSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.TransactionEventTopicName, Constants.Application.RedeemVoucherEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            await _redeemVoucherEventHandler.Handle(Json.Deserialize<RedeemVoucherEvent>(eventRequestPayload), cancellationToken);
        }

        [Function("RollbackVoucherRedemptionEventSubscriber")]
        public async Task Cancel([ServiceBusTrigger(Constants.Application.TransactionEventTopicName, Constants.Application.RollbackVoucherRdmEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            await _rollbackVoucherRdmEventHandler.Handle(Json.Deserialize<RollbackVoucherRdmEvent>(eventRequestPayload), cancellationToken);
        }

        [Function("UpdateVoucherRelatedTxnsPnrEventSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.TransactionEventTopicName, Constants.Application.UpdVchrRelatedTxnsPnrEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            await _patchVoucherRelatedTxnsPnrEventHandler.Handle(Json.Deserialize<PatchVoucherRelatedTxnsPnrEvent>(eventRequestPayload), cancellationToken);
        }
    }
}
