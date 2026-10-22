using System;
using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
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
        public LocalAndUTCDateTime ScheduledDepartureTime { get; set; }
        public LocalAndUTCDateTime ScheduledArrivalTime { get; set; }
        public int? ConnectionTime { get; set; }
        public string NumberOfStops { get; set; }
        public DistanceUnits FlightDistance { get; set; }
        public TimeSpan FlightDuration { get; set; }
        public bool? NextDayIndicator { get; set; }
        public IEnumerable<FlightLeg> Legs { get; set; }
        public string TripType { get; set; }
        public TimeSpan TotalBoundDuration { get; set; }
        public string SegmentNumber { get; set; }
        public string ClassOfService { get; set; }
        public string FareFamily { get; set; }
        public string RevenueClass { get; set; }

    }
}
