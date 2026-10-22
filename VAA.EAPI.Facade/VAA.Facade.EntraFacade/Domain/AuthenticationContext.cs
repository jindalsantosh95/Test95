namespace VAA.Facade.EntraFacade.Domain
{
    public class AuthenticationContext
    {
        public string CorrelationId { get; set; }
        public EntraUser User { get; set; }
    }
}
