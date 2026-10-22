using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Operations.Interfaces;
using VAA.EAPI.GraphQL.Domain.Operations.Queries;
using VAA.EAPI.GraphQL.Domain.Operations.Services;
using OperationsApplication = VAA.EAPI.GraphQL.Domain.Operations.Constants.Application;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;

namespace VAA.EAPI.GraphQL.Domain.Operations
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
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<IPassengerManifestProvider, PassengerManifestProvider_SCVv7>();
            services.AddSingleton<IFlightStatusProvider, FlightStatusProvider>();
        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
            }
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<FlightsQuery>() // todo: add this back in when we need flight search (currently model clash requiring more work)
                       .TryAddType<PassengerManifestQuery>();
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<FlightStatusQuery>();
            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {
        }
    }
}
