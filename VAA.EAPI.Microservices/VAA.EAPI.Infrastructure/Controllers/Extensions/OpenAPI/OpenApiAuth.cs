using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;
using System;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI
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
