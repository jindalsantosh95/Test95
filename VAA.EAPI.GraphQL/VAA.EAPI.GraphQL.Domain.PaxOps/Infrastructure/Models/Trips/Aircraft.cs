using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.OperationalSchedule;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips
{
    public class Aircraft
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string SubfleetCode { get; set; }
        public string? PlaneName { get; set; }
        public string? TailNumber { get; set; }

        public static explicit operator Aircraft((Aircraft aircraft, OperationalScheduleResponse osAircraft) source)
        {
            var (aircraft, osAircraft) = source;
            if (aircraft == null) return null;

            return new Aircraft()
            {
                Code = aircraft.Code,
                Name = aircraft.Name,
                SubfleetCode = aircraft.SubfleetCode,
                PlaneName = osAircraft.Flights?.First()?.Aircraft?.Name,
                TailNumber = osAircraft.Flights?.First()?.Aircraft?.Registration,
            };
        }
    }
}
