using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS
{
    public class RefundPayment : Payment
    {
        public string PaymentTransactionType { get; set; }
        public RefundAmount RefundAmount { get; set; }
        public string CardType { get; set; }
        public string RefundStatus { get; set; }
    }
}
