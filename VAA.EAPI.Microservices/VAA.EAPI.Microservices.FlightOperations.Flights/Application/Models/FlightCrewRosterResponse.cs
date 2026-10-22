using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models
{
    public sealed class FlightCrewRosterResponse
    {
        public static explicit operator FlightCrewRosterResponse(FetchLegMembersResponse source) => new()
        {
            Crew = source.FetchLegMembersResult.LegMembs.TAIMSGetLegMembers?.FMember.AsParallel()
                                                                                   .Select(x => (FlightCrewMember)x)
                                                                                   .ToArray() ?? []
        };

        public IEnumerable<FlightCrewMember> Crew { get; set; } = [];
    }
}
