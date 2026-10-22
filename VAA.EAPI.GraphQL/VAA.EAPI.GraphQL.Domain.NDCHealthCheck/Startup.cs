using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Interfaces;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Queries;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Services;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;

namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck
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
            services.AddSingleton<IHeartBeatService, HeartBeatService>();
            services.AddSingleton<IHealthReportService, HealthReportService>();
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.AddType<NDCHealthCheckQuery>();
            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {

        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            
        }
    }
}
