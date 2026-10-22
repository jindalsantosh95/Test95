using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.SecureFlightInfo
{
    public sealed class AdvancePassengerInformationResponse
    {
        [JsonProperty("trustedTravelerPrograms")]
        public IEnumerable<TrustedTravellerProgramResponse> TrustedTravellerPrograms { get; set; }
    }
}
