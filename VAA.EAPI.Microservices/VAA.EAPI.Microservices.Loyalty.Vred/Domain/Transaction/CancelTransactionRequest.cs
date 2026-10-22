using System;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction
{
    public class CancelTransactionRequest
    {
        public string TransactionId { get; set; }
        public DateTime ActivityDate { get; set; }
        public string TransactionChannel { get; set; }
        public string MemberNumber { get; set; }
        public string CancellationReason { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string Comments { get; set; }
        public string CompensatoryTransactionId { get; set; }
    }
}
