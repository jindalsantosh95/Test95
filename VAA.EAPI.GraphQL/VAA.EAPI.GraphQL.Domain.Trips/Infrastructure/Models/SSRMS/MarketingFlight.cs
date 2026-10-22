using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS
{
    public class MarketingFlight
    {
        public string OperatingSuffix { get; set; }
        public string FlightNumber { get; set; }
        public IEnumerable<Airline> MarketingAirlines { get; set; }
    }
}
