using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.SecureFlightInfo
{
    public sealed class PassengerResponse
    {
        [JsonProperty("passengerResourceId")]
        public string PassengerResourceId { get; set; }

        [JsonProperty("firstNameNum")]
        public string FirstNameNum { get; set; }

        [JsonProperty("lastNameNum")]
        public string LastNameNum { get; set; }

        [JsonProperty("advancePassengerInformation")]
        public AdvancePassengerInformationResponse AdvancePassengerInformation { get; set; }

        [JsonProperty("actionType")]
        public string ActionType { get; set; }

    }
}
