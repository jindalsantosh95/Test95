using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Concierge.Mutations;
using VAA.EAPI.GraphQL.Domain.Concierge.Queries;
using VAA.EAPI.GraphQL.Domain.Concierge.Services;
using VAA.EAPI.GraphQL.Domain.Concierge.Subscriptions;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;

namespace VAA.EAPI.GraphQL.Domain.Concierge
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
            services.AddSingleton<ConciergeMessageService>();
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (!isSecure)
            {
                builder.TryAddType<ConciergeQuery>();
            }
        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (!isSecure)
            {
                builder.TryAddType<ConciergeMutation>();
            }
        }

        public void RegisterSubscriptions(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (!isSecure)
            {
                builder.TryAddType<ConciergeSubscription>();
            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {
        }
    }
}
