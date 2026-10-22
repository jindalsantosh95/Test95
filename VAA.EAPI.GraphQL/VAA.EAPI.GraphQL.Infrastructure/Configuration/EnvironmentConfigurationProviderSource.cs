using Microsoft.Extensions.Configuration;

namespace VAA.EAPI.GraphQL.Infrastructure.Configuration
{
    public sealed class EnvironmentConfigurationProviderSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EnvironmentConfigurationProvider();
        }
    }
}
