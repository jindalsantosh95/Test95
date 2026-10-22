using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using VAA.EAPI.Web.Common.Helper;

namespace VAA.EAPI.Web.Common.Infrastructure.OpenAPI.Configurations
{
    public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
    {
        public override OpenApiInfo Info { get; set; } = new OpenApiInfo()
        {
            Version = GetOpenApiDocVersion(),
            Title = GetOpenApiDocTitle(),
            Description = ConfigReader.Get(Constants.Environment.OpenApiDescription),
        };
        public override OpenApiVersionType OpenApiVersion { get; set; } = GetOpenApiVersion();
    }
}
