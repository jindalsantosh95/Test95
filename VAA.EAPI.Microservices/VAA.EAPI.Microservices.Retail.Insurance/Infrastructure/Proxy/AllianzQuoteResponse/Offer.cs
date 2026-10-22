using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteResponse
{
    public class Offer
    {
        public string ContentCode { get; set; }
        public string ContentType { get; set; }
        public string Id { get; set; }
        public int Sequence { get; set; }
        public Message[] Messages { get; set; }
    }
}
