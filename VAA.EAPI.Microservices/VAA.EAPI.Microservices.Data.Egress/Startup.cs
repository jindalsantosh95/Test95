using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Infrastructure.FunctionHost;

namespace VAA.EAPI.Microservices.Data.Egress
{
    public sealed class Startup : IStartup
    {
        public void ConfigureAppConfiguration(IConfigurationBuilder config)
        {

        }

        public void ConfigureFunctionsWorkerDefaults(IFunctionsWorkerApplicationBuilder worker)
        {

        }

        public void ConfigureServices(IServiceCollection services)
        {

        }
    }
}
