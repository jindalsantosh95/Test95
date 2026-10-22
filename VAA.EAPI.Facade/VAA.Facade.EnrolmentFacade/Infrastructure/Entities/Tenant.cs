using VAA.Facade.EnrolmentFacade.Infrastructure.Entities.Enums;

namespace VAA.Facade.EnrolmentFacade.Infrastructure.Entities
{
    public class Tenant
    {
        public IssuerType Type { get; set; }
        public string Issuer { get; set; }
        public string OpenIdConnectConfiguration { get; set; }
        public string ClientId { get; set; }

    }
}
