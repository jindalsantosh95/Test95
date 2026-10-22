using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Extensions.OpenAPI
{
    public sealed class OpenApiCustomResponseHeader : IOpenApiCustomResponseHeader
    {
        public Dictionary<string, OpenApiHeader> Headers { get; set; } = new Dictionary<string, OpenApiHeader>
        {
            { Common.Constants.Environment.ResponseUrlParName, new OpenApiHeader { Description = "response URL", Schema = new OpenApiSchema { Type = "string" } } }
        };
    }
}
