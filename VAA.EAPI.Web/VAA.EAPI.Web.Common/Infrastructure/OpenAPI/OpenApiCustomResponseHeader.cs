using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.OpenApi.Models;


namespace VAA.EAPI.Web.Common.Infrastructure.OpenAPI
{
    public class OpenApiCustomResponseHeader : IOpenApiCustomResponseHeader
    {
        public Dictionary<string, OpenApiHeader> Headers { get; set; } = new Dictionary<string, OpenApiHeader>
        {
            { Constants.Application.ResponseUrlParName, new OpenApiHeader { Description = "response URL", Schema = new OpenApiSchema { Type = "string" } } }
        };
    }

}
