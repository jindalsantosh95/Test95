using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaSsci
{
    public sealed class BuildTransactionSummaryResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
    }
}
