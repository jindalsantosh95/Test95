using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class FlightSegment
    {
        public string SegmentID { get; set; }
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingCarrier { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public UtcAndLocalDateTime ScheduledDepartureTime { get; set; }
        public UtcAndLocalDateTime ScheduledArrivalTime { get; set; }
        public int? ConnectionTime { get; set; }
        public string NumberOfStops { get; set; }
        public FlightDistance FlightDistance { get; set; }
        public TimeSpan FlightDuration { get; set; }
        public bool? NextDayIndicator { get; set; }
        public IEnumerable<FlightLeg> Legs { get; set; }
        public string TripType { get; set; }
        public TimeSpan TotalBoundDuration { get; set; }
        public string SegmentNumber { get; set; }
        public string ClassOfService { get; set; }
        public string CabinCode { get; set; }
        public string CabinName { get; set; }
        public string CassEquipment { get; set; }
        public Action Status { get; set; }
        public string CurrentActionCode { get; set; }
        public string ElectronicTicketEligibleFlag { get; set; }
        public string FlightNumber { get; set; }
        public string FareFamily { get; set; }

    }
}
