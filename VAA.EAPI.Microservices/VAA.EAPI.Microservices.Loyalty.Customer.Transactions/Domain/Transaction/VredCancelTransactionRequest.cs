using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction
{
    public class VredCancelTransactionRequest
    {
        public string TransactionId { get; set; }
        public string TransactionChannel { get; set; }
        public string MemberNumber { get; set; }
        public string CancellationReason { get; set; }
        public string UpdatedBy { get; set; }
        public string CompensatoryTransactionId { get; set; }

        public static explicit operator VredCancelTransactionRequest((TransactionDocument txnData, string compensatoryId) sourceData)
        {
            if (sourceData.txnData is null) throw new ArgumentNullException(nameof(sourceData.txnData));
            if (sourceData.compensatoryId is null) throw new System.ArgumentNullException(nameof(sourceData.compensatoryId));

            return new VredCancelTransactionRequest
            {
                TransactionId = sourceData.txnData.TransactionId,
                TransactionChannel = sourceData.txnData.TransactionChannel,
                MemberNumber = sourceData.txnData.MemberNumber,
                CancellationReason = sourceData.txnData.CancellationReasonCode,
                UpdatedBy = sourceData.txnData.CancelledBy,
                CompensatoryTransactionId = sourceData.compensatoryId
            };
        }
    }
}
