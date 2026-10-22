using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VAA.EAPI.GraphQL.Infrastructure.FunctionHost
{
    public interface IStartup
    {
        void ConfigureAppConfiguration(IConfigurationBuilder config);

        void ConfigureFunctionsWorkerDefaults(IFunctionsWorkerApplicationBuilder worker);

        void ConfigureServices(IServiceCollection services);

        void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure);

        void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure);

        // Optional so existing domains and hosts remain compatible.
        void RegisterSubscriptions(IRequestExecutorBuilder builder, bool isSecure) { }

        void RegisterHealthChecks(IHealthChecksBuilder builder);
    }
}
