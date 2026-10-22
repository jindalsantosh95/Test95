using System.Collections.Generic;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs
{
    public sealed class CheckAircraftCategoryRequest
    {
        public IEnumerable<string> TailNumbers { get; set; }
        public int CategoryIndex { get; set; }
    }
}
