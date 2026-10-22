using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.TransactionStatement
{
    public class StatementTextValues
    {
        public IEnumerable<AccrualStatements> Accrual { get; set; }
        public IEnumerable<RedemptionStatements> Redemption { get; set; }
        public string Cancellation { get; set; }
    }
}
