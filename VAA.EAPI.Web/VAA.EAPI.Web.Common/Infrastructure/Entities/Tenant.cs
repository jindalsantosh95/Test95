using VAA.EAPI.Web.Common.Infrastructure.Entities.Enums;

namespace VAA.EAPI.Web.Common.Infrastructure.Entities
{
    public class Tenant
    {
        public IssuerType Type { get; set; }
        public string Issuer { get; set; }
        public string OpenIdConnectConfiguration { get; set; }
        public string ClientId { get; set; }

    }
}
