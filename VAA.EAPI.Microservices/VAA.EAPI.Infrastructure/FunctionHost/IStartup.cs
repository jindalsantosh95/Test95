using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VAA.EAPI.Infrastructure.FunctionHost
{
    public interface IStartup
    {
        void ConfigureAppConfiguration(IConfigurationBuilder config);

        void ConfigureFunctionsWorkerDefaults(IFunctionsWorkerApplicationBuilder worker);

        void ConfigureServices(IServiceCollection services);
    }
}
