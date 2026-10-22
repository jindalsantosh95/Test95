using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.VoucherMonitoring
{
    public class VoucherResult
    {
        public string VoucherNumber { get; set; }
        public string VoucherStatus { get; set; }
        public VoucherTxnProcessingStatus TransactionProcessingStatus { get; set; }
        public int TransactionFound { get; set; }
        public string Summary { get; set; }
        public IEnumerable<VoucherTransactionInfo> Transactions { get; set; }
    }
}

