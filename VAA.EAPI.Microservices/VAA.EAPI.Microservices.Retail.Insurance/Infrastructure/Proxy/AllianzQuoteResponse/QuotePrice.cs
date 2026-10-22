using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteResponse
{
    public class QuotePrice
    {
        public bool IsAvailable { get; set; }
        public bool IsIncluded { get; set; }
        public double Price { get; set; }
        public string StepName { get; set; }
        public string StepType { get; set; }    
        public Sequence[] Sequences { get; set; }
    }

}
