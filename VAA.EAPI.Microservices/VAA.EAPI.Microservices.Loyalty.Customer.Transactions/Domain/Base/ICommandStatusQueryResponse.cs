using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base
{
    public interface ICommandStatusQueryResponse
    {
        public string RequestId { get; }
        public IEnumerable<ErrorDetails> Errors { get; }
        public Status CommandStatus { get; }
    }
}
