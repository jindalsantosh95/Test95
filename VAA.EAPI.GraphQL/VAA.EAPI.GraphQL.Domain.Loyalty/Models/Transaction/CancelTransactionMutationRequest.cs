using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class CancelTransactionMutationRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string CancellationReason { get; set; }
        public string InitiatedBy { get; set; }
        public string TransactionSource { get; set; }
        public string InitiatedTransactionId { get; set; }
        public IEnumerable<CancelTransactionDetails> CancelTransactionDetails { get; set; }
    }
}
