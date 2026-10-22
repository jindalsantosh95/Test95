using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Flight.Services.Domain.PopsMeal
{
    public class MealCancelRequest
    {
        public string OrderId { get; set; }
        public string RecordLocator { get; set; }
        public string CancelReason { get; set; }
        public IEnumerable<FlightSegment> Segments { get; set; } 
    }

}