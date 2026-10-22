using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Security.Interfaces;
using VAA.EAPI.GraphQL.Domain.Security.Mutations;
using VAA.EAPI.GraphQL.Domain.Security.Queries;
using VAA.EAPI.GraphQL.Domain.Security.Services;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;

namespace VAA.EAPI.GraphQL.Domain.Security
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
            services.AddSingleton<IAPIMSubscriptionMutationProvider, APIMSubscriptionProvider>()
                    .AddSingleton<IAPIMSubscriptionQueryProvider, APIMSubscriptionProvider>();
        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<APIMSubscriptionNDCMutation>();
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {

            }
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<APIMSubscriptionNDCQuery>();
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {

            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {

        }
    }
}
