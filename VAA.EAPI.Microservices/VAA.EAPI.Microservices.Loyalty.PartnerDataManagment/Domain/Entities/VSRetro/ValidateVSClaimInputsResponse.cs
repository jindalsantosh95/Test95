using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro
{
    public class ValidateVSClaimInputsResponse
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Success { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
