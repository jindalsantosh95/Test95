using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.PnrOrderValidation
{
    public sealed class PnrOrderValidationResponse
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Pnr { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsGroup { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool FirstName { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Surname { get; set; }
    }
}
