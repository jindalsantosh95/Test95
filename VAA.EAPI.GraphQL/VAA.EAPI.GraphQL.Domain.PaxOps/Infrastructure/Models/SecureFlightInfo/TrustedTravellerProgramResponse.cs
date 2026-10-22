using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.SecureFlightInfo
{
    public sealed class TrustedTravellerProgramResponse
    {
        [JsonProperty("program")]
        public string Program { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }
}
