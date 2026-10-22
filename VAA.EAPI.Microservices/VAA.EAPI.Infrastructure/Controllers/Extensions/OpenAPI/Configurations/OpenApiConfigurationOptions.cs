using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Constants;

namespace VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI.Configurations
{
    public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
    {
        public override OpenApiInfo Info { get; set; } = new OpenApiInfo()
        {
            Version = GetOpenApiDocVersion(),
            Title = GetOpenApiDocTitle(),
            Description = ConfigReader.Try(Environment.OpenApiDescription, "OpenAPI documentation"),
        };
        public override OpenApiVersionType OpenApiVersion { get; set; } = GetOpenApiVersion();
    }
}
