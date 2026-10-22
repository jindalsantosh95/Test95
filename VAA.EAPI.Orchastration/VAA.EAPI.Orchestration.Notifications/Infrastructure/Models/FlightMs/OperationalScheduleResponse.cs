using System.Collections.Generic;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs
{
    public class OperationalScheduleResponse
    {
        public IEnumerable<Flight> Flights { get; set; }
    }
}
