using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers
{
    public class VoucherTxnsPnrUpdateCommandQueryResponse : ICommandStatusQueryResponse
    {
        public string RequestId { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public IEnumerable<TransactionDTO> Transactions { get; set; }
        public Status CommandStatus { get; set; }
    }
}
