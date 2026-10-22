using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteRequest
{
    public class QuoteTravel
    {
        public string BrandedFare { get; set; }
        public string ClassOfService { get; set; }
        public DateTime DepartureDate { get; set; }
        public OriginDestinationDetails Destination { get; set; }
        public bool IncludesConnection { get; set; }
        public bool IsDomestic { get; set; }
        public string MembershipStatus { get; set; }
        public OriginDestinationDetails Origin { get; set; }
        public string Refundable { get; set; }
        public DateTime ReturnDate { get; set; }
        public double TotalCost { get; set; }
        public string TravelAgent { get; set; }
        public Travelsegment[] TravelSegments { get; set; }
        public string TripIndicator { get; set; }
    }


}
