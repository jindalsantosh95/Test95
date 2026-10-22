using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS
{
    public class TransactionServiceResponse
    {
        public string RequestId { get; set; }
        public string Status { get; set; }
        public DateTime TimeStamp { get; set; }
        public IEnumerable<ErrorDetails> ErrorDetails { get; set; }
    }
}
