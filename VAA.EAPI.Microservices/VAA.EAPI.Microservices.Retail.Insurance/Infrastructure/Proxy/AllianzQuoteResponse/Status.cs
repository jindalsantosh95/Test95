using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteResponse
{
    public class Status
    {
        public string Code { get; set; }
        public string CreationDate { get; set; }
        public string ProcessId { get; set; }
        public string Version { get; set; }
        public string Source { get; set; }
        public string ErrorText { get; set; }
        public string Reason {get; set;}
        public string Message { get; set; }
    }
}
