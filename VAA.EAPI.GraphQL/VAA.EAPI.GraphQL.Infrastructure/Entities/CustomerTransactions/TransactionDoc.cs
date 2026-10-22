using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions
{
    public class TransactionDoc
    {
        public string Comments { get; set; }
        public string TransactionValue { get; set; }
        public string PartnerId { get; set; }
        public string TransactionDate { get; set; }
        public string ProductId { get; set; }
        public string TransactionId { get; set; }
        public string StatusCode { get; set; }
        public string TransactionSubType { get; set; }
        public string ActivityDate { get; set; }
        public string TransactionType { get; set; }
        public string StatementText { get; set; }
        public string TransactionChannel { get; set; }
        public IEnumerable<CreditDetail> CreditDetails { get; set; }
    }
}
