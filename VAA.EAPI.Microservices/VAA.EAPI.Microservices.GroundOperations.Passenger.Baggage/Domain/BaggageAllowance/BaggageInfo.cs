using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.BaggageAllowance
{
    public class BaggageInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public IconType? IconType { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> Description { get; set; }
    }
}