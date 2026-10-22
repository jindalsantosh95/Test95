using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteResponse
{

    public class QuoteResponse
    {
        public Status Status { get; set; }
        public Product[] Products { get; set; }
        public Location[] Locations { get; set; }
        public Transactiondetails TransactionDetails { get; set; }
    }
}
