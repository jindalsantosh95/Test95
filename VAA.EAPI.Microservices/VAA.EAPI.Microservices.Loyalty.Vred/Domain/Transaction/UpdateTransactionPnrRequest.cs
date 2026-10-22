using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction
{
    public class UpdateTransactionPnrRequest
    {
        public string MemberNumber { get; set; }
        public string UpdatedBy { get; set; }
        public IEnumerable<UpdateTransactionPnrDetails> TransactionDetails { get; set; }
    }
}
