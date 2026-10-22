using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions
{
    public class TransactionResponse<T>
    {
        public string customerId { get; set; }

        public IEnumerable<T> transactionDocuments { get; set; }
    }
}
