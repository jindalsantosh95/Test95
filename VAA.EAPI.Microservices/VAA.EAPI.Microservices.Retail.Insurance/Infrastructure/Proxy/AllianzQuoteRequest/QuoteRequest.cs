using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteRequest
{
    public class QuoteRequest
    {
        public Credentials Credentials { get; set; }
        public Customer[] Customers { get; set; }
        public Gadget Gadget { get; set; }
        public Partner Partner { get; set; }
        public Requestdetail RequestDetail { get; set; }
        public QuoteTravel Travel { get; set; }
    }
}
