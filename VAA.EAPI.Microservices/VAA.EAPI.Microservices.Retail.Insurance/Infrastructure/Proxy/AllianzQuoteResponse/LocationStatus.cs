using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteResponse
{
    
    public class LocationStatus
    {
        public string Code { get; set; }
        public string ErrorText { get; set; }
        public string Message { get; set; }
        public string Reason { get; set; }
    }
}
