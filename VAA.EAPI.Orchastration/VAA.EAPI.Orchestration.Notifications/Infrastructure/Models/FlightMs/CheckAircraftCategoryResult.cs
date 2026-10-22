using System.Collections.Generic;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs
{
    public sealed class CheckAircraftCategoryResult
    {
        public IEnumerable<AircraftCategoryCheck> AircraftCategoryChecks { get; set; }
    }

    public sealed class AircraftCategoryCheck
    {
        public string TailNumber { get; set; }
        public bool DoesCategoryExist { get; set; }
    }
}
