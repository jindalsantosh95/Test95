using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;
using System;

namespace VAA.EAPI.GraphQL.Extensions.OpenAPI
{
    public sealed class OpenApiAuth : OpenApiOAuthSecurityFlows
    {
        public OpenApiAuth()
        {
            ClientCredentials = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri(Common.Constants.Environment.OpenApiAuthTokenUrl),
                Scopes = { { Common.Constants.Environment.OpenApiScopes, "Default scope defined in the app" } }
            };
        }
    }
}
