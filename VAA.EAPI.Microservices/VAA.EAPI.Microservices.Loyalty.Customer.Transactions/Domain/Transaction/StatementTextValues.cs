using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction
{
    public class StatementTextValues
    {
        public IEnumerable<AccrualStatements> Accrual { get; set; }
        public IEnumerable<RedemptionStatements> Redemption { get; set; }
        public string Cancellation { get; set; }
    }
}
