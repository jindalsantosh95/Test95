using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteRequest
{
    public class Customer
    {
        public Address Address { get; set; }
        public int Age { get; set; }
        public string CustomerId { get; set; }
        public string CustomerType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Sequence { get; set; }
    }
}
