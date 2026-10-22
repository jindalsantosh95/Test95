using Microsoft.Extensions.Configuration;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Infrastructure.Configuration
{
    public static class EnvironmentConfigurationExtensions
    {
        public static IConfigurationBuilder AddEnvironmentConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            // Defined by the azure platform, if not present signals process is running locally
            if (ConfigReader.Try("WEBSITE_INSTANCE_ID").IsNullOrWhiteSpace())
                configurationBuilder.Add(new EnvironmentConfigurationProviderSource());

            return configurationBuilder;
        }
    }
}
