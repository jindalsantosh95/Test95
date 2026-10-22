using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models
{
    public sealed class CheckAircraftCategoryResult
    {
        public string TailNumber { get; set; } = string.Empty;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool DoesCategoryExist { get; set; }
    }
}
