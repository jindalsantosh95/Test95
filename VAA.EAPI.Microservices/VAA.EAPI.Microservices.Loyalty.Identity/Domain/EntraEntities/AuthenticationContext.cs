namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities
{
    public class AuthenticationContext
    {
        public string CorrelationId {  get; set; }
        public EntraUser User { get; set; }
    }
}
