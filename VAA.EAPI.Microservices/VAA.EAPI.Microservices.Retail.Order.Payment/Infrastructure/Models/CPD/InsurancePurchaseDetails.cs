using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class InsurancePurchaseDetails
    {
        public InsurancePurchaseStatus InsurancePurchaseStatus { get; set; }
        public decimal? InsurancePurchaseAmount { get; set; }
        public string TransactionId { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
