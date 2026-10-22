using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteRequest
{
    public class Travelsegment
    {
        public OriginDestinationDetails Destination { get; set; }
        public OriginDestinationDetails Origin { get; set; }
        public string RouteGroup { get; set; }
        public int Sequence { get; set; }
        public Travelleg[] TravelLegs { get; set; }
    }
}
