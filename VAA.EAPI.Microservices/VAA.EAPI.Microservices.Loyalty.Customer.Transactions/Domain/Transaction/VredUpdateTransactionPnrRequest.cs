using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction
{
    public class VredUpdateTransactionPnrRequest
    {
        public string MemberNumber { get; set; }
        public string UpdatedBy { get; set; }
        public IEnumerable<VredUpdateTransactionPnrDetails> TransactionDetails { get; set; }

        public static explicit operator VredUpdateTransactionPnrRequest((PatchVoucherRelatedTxnsPnrRequest eventData, IEnumerable<VredUpdateTransactionPnrDetails> txnsDetail) sourceData)
        {
            if (sourceData.eventData is null) throw new ArgumentNullException(nameof(sourceData.eventData));
            if (sourceData.txnsDetail is null) throw new ArgumentNullException(nameof(sourceData.txnsDetail));

            return new VredUpdateTransactionPnrRequest
            {
                MemberNumber = sourceData.eventData.MemberNumber,
                UpdatedBy = sourceData.eventData.InitiatedBy,
                TransactionDetails = sourceData.txnsDetail
            };
        }
    }
}
