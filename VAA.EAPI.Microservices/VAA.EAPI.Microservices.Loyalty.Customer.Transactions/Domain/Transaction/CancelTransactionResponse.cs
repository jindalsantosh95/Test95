using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction
{
    public class CancelTransactionResponse
    {
        public IEnumerable<TransactionDocument> Transactions { get; set; }
    }

}
