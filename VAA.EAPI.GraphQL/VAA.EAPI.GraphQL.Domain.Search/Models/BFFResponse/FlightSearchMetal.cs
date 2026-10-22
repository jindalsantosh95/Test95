using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class FlightSearchMetal
    {
        public string? family { get; set; }

        public string? name { get; set; }

        public FlightSearchMetal() { }
        public FlightSearchMetal(string iataCode, IEnumerable<AircraftInfo> aircraftInfo)
        {
            family = "787"; //TODO: Family should map from AirCraft RefMS
            name = aircraftInfo.FirstOrDefault(ai => ai.IataCode == iataCode)?.FullName ?? iataCode;
        }
    }
}
