using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;


namespace VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI
{
    public class OpenApiCustomResponseHeader : IOpenApiCustomResponseHeader
    {
        public Dictionary<string, OpenApiHeader> Headers { get; set; } = new Dictionary<string, OpenApiHeader>
        {
            { Constants.Constants.ResponseUrlParName, new OpenApiHeader { Description = "response URL", Schema = new OpenApiSchema { Type = "string" } } }
        };
    }

}
