using System.Collections.Generic;
using System;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightOrderNotification
{
    public class Segment
    {
        public Carrier OperatingCarrier { get; set; }
        public Carrier MarketingCarrier { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledDepartureUtcDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public DateTime ScheduledArrivalUtcDateTime { get; set; }
        public Airport Origin { get; set; }
        public Airport Destination { get; set; }
        public IEnumerable<Leg> Legs { get; set; }

        public static explicit operator Segment(SegmentDetail segmentDetail)
        {
            if (segmentDetail == null) return null;

            return new Segment
            {
                OperatingCarrier = new Carrier
                {
                    CarrierCode = segmentDetail.OperatingCarrier ?? segmentDetail.AirlineCode,
                    FlightNum = segmentDetail.OperatingFlightNumber ?? segmentDetail.FlightNumber
                },
                MarketingCarrier = new Carrier
                {
                    CarrierCode = segmentDetail.MarketingCarrier ?? segmentDetail.AirlineCode,
                    FlightNum = segmentDetail.MarketingFlightNumber ?? segmentDetail.FlightNumber
                },
                ScheduledDepartureLocalDateTime = DateTimeParser.Parse(segmentDetail.DepartureDate, segmentDetail.DepartureTime),
                ScheduledDepartureUtcDateTime = DateTimeParser.Parse(segmentDetail.DepartureDate, segmentDetail.DepartureTime).ToUniversalTime(),
                ScheduledArrivalLocalDateTime = DateTimeParser.Parse(segmentDetail.LegArrivalDate ?? segmentDetail.DepartureDate, segmentDetail.ArrivalTime),
                ScheduledArrivalUtcDateTime = DateTimeParser.Parse(segmentDetail.LegArrivalDate ?? segmentDetail.DepartureDate, segmentDetail.ArrivalTime).ToUniversalTime(),
                Origin = (Airport)segmentDetail.DeparturePoint,
                Destination = (Airport)segmentDetail.ArrivalPoint
            };
        }



        
    }
}