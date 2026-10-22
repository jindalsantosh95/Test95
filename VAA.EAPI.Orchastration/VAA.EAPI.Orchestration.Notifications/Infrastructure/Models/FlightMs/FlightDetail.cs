using System;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs
{
    public sealed class FlightDetail
    {
        public string Id => $"{Date:yyyyMMdd}{ScheduledTimeOfDeparture:HHmm}{FlightNo}";
        public string Carrier { get; set; }
        public int FlightNo { get; set; }
        public TimeOnly ScheduledTimeOfDeparture { get; set; }
        public DateOnly Date { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public string Registration { get; set; }
        public DateTime ScheduledDateTimeOfDeparture => Date.ToDateTime(ScheduledTimeOfDeparture);
    }
}
