using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{
    public class Multiprequaloffersstatu
    {
        public string ApplicationKey { get; set; }
        public string CreditLimit { get; set; }
        public string TierCode { get; set; }
        public string ProductCode { get; set; }
        public string FlowType { get; set; }
        public string PromoCode { get; set; }
        public string PromoType { get; set; }
    }
}
