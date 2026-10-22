using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities
{
    public class ClaimsResponse
    {        
        public string CorrelationId { get; set; }
        public string MemberNumber { get; set; }
        public string MemberStatus { get; set; }       
        public string MemberEmail { get; set; }       
    }
}
