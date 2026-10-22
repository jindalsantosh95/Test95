using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction
{
    public class TransactionCommandStatusResponse
    {
        public string RequestId { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public TransactionDocument Transaction { get; set; }
        public Status CommandStatus { get; set; }
    }
}
