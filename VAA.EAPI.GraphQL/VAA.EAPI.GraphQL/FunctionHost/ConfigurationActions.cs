using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace VAA.EAPI.GraphQL.FunctionHost
{
    public class ConfigurationActions
    {
        public Action<IConfigurationBuilder> ConfigureAppConfiguration { get; }
        public Action<IFunctionsWorkerApplicationBuilder> ConfigureFunctionsWorkerDefaults { get; }
        public Action<IServiceCollection> ConfigureServices { get; }
        public Action<IRequestExecutorBuilder, bool> RegisterQueries { get; }
        public Action<IRequestExecutorBuilder, bool> RegisterMutations { get; }
        public Action<IRequestExecutorBuilder, bool> RegisterSubscriptions { get; }
        public Action<IHealthChecksBuilder> RegisterHealthChecks { get; }

        public ConfigurationActions(
            Action<IConfigurationBuilder> configureAppConfiguration,
            Action<IFunctionsWorkerApplicationBuilder> configureFunctionsWorkerDefaults,
            Action<IServiceCollection> configureServices,
            Action<IRequestExecutorBuilder, bool> registerQueries,
            Action<IRequestExecutorBuilder, bool> registerMutations,
            Action<IHealthChecksBuilder> registerHealthChecks,
            Action<IRequestExecutorBuilder, bool> registerSubscriptions = null)
        {
            ConfigureAppConfiguration = configureAppConfiguration;
            ConfigureFunctionsWorkerDefaults = configureFunctionsWorkerDefaults;
            ConfigureServices = configureServices;
            RegisterQueries = registerQueries;
            RegisterMutations = registerMutations;
            RegisterHealthChecks = registerHealthChecks;
            RegisterSubscriptions = registerSubscriptions ?? ((_, _) => { });
        }
    }
}
