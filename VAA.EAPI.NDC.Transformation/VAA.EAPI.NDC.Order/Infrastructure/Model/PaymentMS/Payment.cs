using System.Collections.Generic;
using VAA.EAPI.NDC.Order.Constants;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS
{
    public class Payment : PaymentDetails
    {
        public string PaymentTransactionId { get; set; }
        public string PaymentApprovalCode { get; set; }
    }
}