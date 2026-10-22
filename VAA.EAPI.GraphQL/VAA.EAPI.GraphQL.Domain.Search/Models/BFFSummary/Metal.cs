using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public class Metal
    {
        public string? Family { get; set; }
        public string? Name { get; set; }

        public Metal() { }
        public Metal(string iataCode, IEnumerable<AircraftInfo> aircraftInfo)
        {
            Family = "787"; //TODO: Family should map from AirCraft RefMS
            Name = aircraftInfo.FirstOrDefault(ai => ai.IataCode == iataCode)?.FullName ?? iataCode;
        }
    }
}