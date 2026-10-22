using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction
{
    public class TransactionResponse
    {
        public IEnumerable<TransactionDocument> TransactionDocuments { get; set; }
        public string CustomerId { get; set; }
    }
}
