using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS
{
    public class Legs
    {
        public UtcAndLocalDateTime ScheduledDepartureTime { get; set; }
        public string LegSequenceNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string OperatingCarrier { get; set; }
        public UtcAndLocalDateTime ScheduledArrivalTime { get; set; }
        public string Destination_Airport_Code { get; set; }
        public string LegDuration { get; set; }
        public string StopOverDuration { get; set; }
        public FlightDistance LegDistance { get; set; }
        public string OriginTerminal { get; set; }
        public string DestinationTerminal { get; set; }
    }
}
