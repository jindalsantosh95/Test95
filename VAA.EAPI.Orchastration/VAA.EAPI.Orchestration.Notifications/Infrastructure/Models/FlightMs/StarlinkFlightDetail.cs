using System;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs
{
    public sealed class StarlinkFlightDetail
    {
        public string Id => $"Starlink-{Date:yyyyMMdd}{ScheduledTimeOfDeparture:HHmm}{FlightNo}-{TailNumber}";
        public string Carrier { get; set; }
        public int FlightNo { get; set; }
        public TimeOnly ScheduledTimeOfDeparture { get; set; }
        public DateOnly Date { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public string TailNumber { get; set; }
        public DateTime ScheduledDateTimeOfDeparture => Date.ToDateTime(ScheduledTimeOfDeparture);

        public static explicit operator StarlinkFlightDetail(FlightDetail source) => new()
        {
            Carrier = source.Carrier,
            FlightNo = source.FlightNo,
            ScheduledTimeOfDeparture = source.ScheduledTimeOfDeparture,
            Date = source.Date,
            Departure = source.Departure,
            Arrival = source.Arrival,
            TailNumber = source.Registration
        };
    }
}
