using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteResponse
{
    public class Message
    {
        public int MessageSequence { get; set; }
        public string MessageTag { get; set; }
        public string CompiledMessage { get; set; }
    }
}
