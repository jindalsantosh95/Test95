using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteRequest
{
    public class Travelleg
    {
        public string ClassOfService { get; set; }
        public OriginDestinationDetails Destination { get; set; }
        public string FareClass { get; set; }
        public OriginDestinationDetails Origin { get; set; }
        public string RouteNumber { get; set; }
        public int Sequence { get; set; }
    }
}
