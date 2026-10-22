using Newtonsoft.Json;
using System.Net.Http;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class TargetedMembershipMutationRequest
    {
        [JsonIgnore]
        public string CustomerId { get; set; }
        [JsonIgnore]
        public HttpMethod RequestType { get; set; }
        public TargetedMembership TargetedMembershipDocument { get; set; }
    }
}
