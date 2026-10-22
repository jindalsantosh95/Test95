using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaSsci
{
    public sealed class BuildTransactionSummaryRequest
    {
        [JsonProperty("recordLocator")]
        public string RecordLocator { get; set; } = string.Empty;

        [JsonProperty("originAirportCode")]
        public string OriginAirportCode { get; set; } = string.Empty;

        [JsonProperty("transactionError")]
        public string TransactionError { get; set; } = string.Empty;
    }
}
