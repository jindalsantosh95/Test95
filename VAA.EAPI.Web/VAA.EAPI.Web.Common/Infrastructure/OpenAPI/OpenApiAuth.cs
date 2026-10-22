using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;
using VAA.EAPI.Web.Common.Helper;

namespace VAA.EAPI.Web.Common.Infrastructure.OpenAPI
{
    public class OpenApiAuth : OpenApiOAuthSecurityFlows
    {
        public OpenApiAuth()
        {
            ClientCredentials = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri(ConfigReader.Get(Constants.Environment.OpenApiTokenUrl)),
                Scopes = { { ConfigReader.Get(Constants.Environment.OpenApiScope), "Default scope defined in the app" } }
            };
        }
    }

}
