using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Core.Azure.Functions.Configuration
{
    public static class EnvironmentConfigurationExtensions
    {
        public static IConfigurationBuilder AddEnvironmentConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            // Defined by the azure platform, if not present signals process is running locally
            if (ConfigReader.Get("WEBSITE_INSTANCE_ID").IsNullOrWhiteSpace())
                configurationBuilder.Add(new EnvironmentConfigurationProviderSource());

            return configurationBuilder;
        }
    }
}
