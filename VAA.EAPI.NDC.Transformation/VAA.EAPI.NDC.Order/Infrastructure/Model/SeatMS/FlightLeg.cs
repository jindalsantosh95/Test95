using System;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class FlightLeg
    {
        public string AircraftTypeCode { get; set; }
        public string AircraftTypeName { get; set; }
        public int? LegSequenceNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public UtcAndLocalDateTime ScheduledDepartureTime { get; set; }
        public UtcAndLocalDateTime ScheduledArrivalTime { get; set; }
        public string EquipmentType { get; set; }
        public FlightDistance LegDistance { get; set; }
        public TimeSpan LegDuration { get; set; }
        public string DelegatedCarrier { get; set; }
        public string OperatingCarrier { get; set; }

    }
}
