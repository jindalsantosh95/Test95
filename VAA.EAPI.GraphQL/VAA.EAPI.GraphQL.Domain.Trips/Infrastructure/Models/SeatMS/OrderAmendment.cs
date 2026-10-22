using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    public class OrderAmendment
    {            
        public string ProductType { get; set; }
        public IEnumerable<FlightItems> FlightItems { get; set; }
        public IEnumerable<string> CustomerReference { get; set; }
    }
}