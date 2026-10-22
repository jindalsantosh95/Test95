using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class Leg
    {
        public string LegSequenceNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }

        // Input-only alias for the persisted SCV field; keep the API name unchanged.
        [Newtonsoft.Json.JsonProperty("Destination_Airport_Code")]
        private string ScvDestinationAirportCode { set => DestinationAirportCode = value; }
        public ScheduledDepartureTime ScheduledDepartureTime { get; set; }
        public ScheduledArrivalTime ScheduledArrivalTime { get; set; }
        public string EquipmentType { get; set; }
        public LegDistance LegDistance { get; set; }
        public string LegDuration { get; set; }
        public string DelegatedCarrier { get; set; }
        public string OperatingCarrier { get; set; }
    }
}
