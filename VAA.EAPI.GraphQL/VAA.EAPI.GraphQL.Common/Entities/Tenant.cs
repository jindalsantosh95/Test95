using VAA.EAPI.GraphQL.Common.Entities.Enums;

namespace VAA.EAPI.GraphQL.Common.Entities
{
    public class Tenant
    {
        public IssuerType Type { get; set; }
        public string Issuer { get; set; }
        public string OpenIdConnectConfiguration { get; set; }
        public string[] ClientId { get; set; }

    }
}
