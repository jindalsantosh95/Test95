using Microsoft.Extensions.Configuration;

namespace VAA.EAPI.Automation.Core.Azure.Functions.Configuration
{
    public sealed class EnvironmentConfigurationProviderSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EnvironmentConfigurationProvider();
        }
    }
}
