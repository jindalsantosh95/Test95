using System;
using System.Linq;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;

namespace VAA.EAPI.Orchestration.Notifications.Domain.Entities
{
    public class FlightOrderDataRequest
    {

        public static explicit operator FlightOrderDataRequest(FlightDetail source)
        {
            return new FlightOrderDataRequest
            {
                Departure = source.Departure,
                Arrival = source.Arrival,
                OperatingCarrier = new Carrier
                {
                    CarrierCode = source.Carrier,
                    FlightNum = source.FlightNo.ToString()
                },
                ScheduledDepartureUtcDateTime = source.ScheduledDateTimeOfDeparture,
                NotificationType = Constants.Constants.NotificationTypeCheckInOpen
            };
        }

        public static explicit operator FlightOrderDataRequest(StarlinkFlightDetail source)
        {
            return new FlightOrderDataRequest
            {
                Departure = source.Departure,
                Arrival = source.Arrival,
                OperatingCarrier = new Carrier
                {
                    CarrierCode = source.Carrier,
                    FlightNum = source.FlightNo.ToString()
                },
                ScheduledDepartureUtcDateTime = source.ScheduledDateTimeOfDeparture,
                NotificationType = Constants.Constants.NotificationTypeStarlink
            };
        }

        public static explicit operator FlightOrderDataRequest(Infrastructure.Models.FlightMs.Flight source)
        {
            var legsList = source.Legs?.ToList();
            var firstLeg = legsList?.FirstOrDefault();
            var lastLeg = legsList?.LastOrDefault();

            return new FlightOrderDataRequest
            {
                Departure = firstLeg?.OriginAirportCode,
                Arrival = lastLeg?.DestinationAirportCode,
                OperatingCarrier = new Carrier
                {
                    CarrierCode = source.Carrier,
                    FlightNum = source.FlightNumber
                },
                ScheduledDepartureLocalDateTime = firstLeg?.ScheduledDepartureTime?.LocalDateTime ?? default,
                ScheduledArrivalLocalDateTime = lastLeg?.ScheduledArrivalTime?.LocalDateTime ?? default,
                NotificationType = Constants.Constants.NotificationTypeAircraftSwap
            };
        }

        public string Departure { get; set; }
        public string Arrival { get; set; }
        public Carrier OperatingCarrier { get; set; }
        public DateTime ScheduledDepartureUtcDateTime { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public string NotificationType { get; set; } = Constants.Constants.NotificationTypeCheckInOpen;
    }
}
