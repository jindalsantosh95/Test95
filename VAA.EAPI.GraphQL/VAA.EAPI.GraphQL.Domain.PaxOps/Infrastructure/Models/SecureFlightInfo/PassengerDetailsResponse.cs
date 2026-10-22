using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.SecureFlightInfo
{
    public sealed class PassengerDetailsResponse
    {
        [JsonProperty("passengerInformation")]
        public PassengerInformationResponse PassengerInformation { get; set; }
    }
}
