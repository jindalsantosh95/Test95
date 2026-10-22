using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Flight.Services.Domain
{
    public class FlightServiceRequest
    {
        public Parties Parties { get; set; }
        public Pos Pos { get; set; }
        public IEnumerable<Segment> FlightSegments { get; set; }
        public string OfferId { get; set; }
        public string OrderId { get; set; }
        public string ChannelId { get; set; }
        public string RecordLocator { get; set; }
    }
}
