using System;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs
{
    public sealed class FlightScheduleRequest
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
