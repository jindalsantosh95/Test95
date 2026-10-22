using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS
{
    public class SplitPaymentRS
    {
        public IEnumerable<PaymentDocument> PaymentDocument { get; set; }

        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
