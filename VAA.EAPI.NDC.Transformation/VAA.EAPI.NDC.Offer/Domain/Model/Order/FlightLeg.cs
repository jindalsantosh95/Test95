using System;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class FlightLeg
    {
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public LocalAndUTCDateTime ScheduledDepartureTime { get; set; }
        public LocalAndUTCDateTime ScheduledArrivalTime { get; set; }
        public string EquipmentType { get; set; }
        public DistanceUnits LegDistance { get; set; }
        public TimeSpan LegDuration { get; set; }
        public string DelegatedCarrier { get; set; }
        public string OperatingCarrier { get; set; }
        public string AircraftTypeCode { get; set; }
        public int? LegSequenceNumber { get; set; }
        public string AircraftTypeName { get; set; }


    }
}
