using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Extentions;

namespace VAA.EAPI.Infrastructure.FunctionHost
{
    public sealed class HostBuilderInitialiser
    {
        public static explicit operator HostBuilderInitialiser(HostBuilder source) => new(source);

        static HostBuilderInitialiser()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, args) =>
            {
                var root = ((AppDomain)s).BaseDirectory;

                var assemblyName = new AssemblyName(args.Name);

                var path = Directory.GetFiles(root, $"{assemblyName.Name}*.dll").FirstOrDefault();

                return path.IsNullOrWhiteSpace() ? default : Assembly.LoadFrom(path);
            };
        }

        private readonly IEnumerable<Assembly> assemblies;
        private readonly HostBuilder hostBuilder;
        private readonly string[] commandLineArguments = Array.Empty<string>();
        private readonly IEnumerable<Action<IConfigurationBuilder>> configureAppConfiguration = Enumerable.Empty<Action<IConfigurationBuilder>>();
        private readonly IEnumerable<Action<IFunctionsWorkerApplicationBuilder>> configureFunctionsWorkerDefaults = Enumerable.Empty<Action<IFunctionsWorkerApplicationBuilder>>();
        private readonly IEnumerable<Action<IServiceCollection>> configureServices = Enumerable.Empty<Action<IServiceCollection>>();

        private HostBuilderInitialiser(HostBuilder hostBuilder)
        {
            this.hostBuilder = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            commandLineArguments = this.hostBuilder.Properties.ContainsKey("args") ? (string[])hostBuilder.Properties["args"] : Array.Empty<string>();

            assemblies = Directory.GetFiles(".", "VAA.EAPI.Microservices.*.dll", SearchOption.TopDirectoryOnly)
                                  .Distinct()
                                  .Select(x => Assembly.LoadFile(Path.GetFullPath(x)));

            var startups = assemblies.Where(x => x.ExportedTypes.Any(t => t.Name.Is("Startup") && t.GetInterface("IStartup") != null))
                                     .Select(x =>
                                     {
                                         var type = x.ExportedTypes.First(t => t.Name.Is("Startup") && t.GetInterface("IStartup") != null);

                                         var instance = (IStartup)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);

                                         return new Tuple<Action<IConfigurationBuilder>, Action<IFunctionsWorkerApplicationBuilder>, Action<IServiceCollection>>(instance.ConfigureAppConfiguration, instance.ConfigureFunctionsWorkerDefaults, instance.ConfigureServices);
                                     })
                                     .ToArray();

            configureAppConfiguration = startups.Select(x => x.Item1);
            configureFunctionsWorkerDefaults = startups.Select(x => x.Item2);
            configureServices = startups.Select(x => x.Item3);
        }

        public IHostBuilder Configure() => hostBuilder
            .ConfigureAppConfiguration(builder =>
            {
                if (commandLineArguments.Any())
                    builder.AddCommandLine(commandLineArguments);

                foreach (var invoke in configureAppConfiguration)
                    invoke(builder);
            })
            .ConfigureFunctionsWorkerDefaults(worker =>
            {
                worker.AddDefaultMiddleware();

                foreach (var invoke in configureFunctionsWorkerDefaults)
                    invoke(worker);
            }, options =>
            {
                options.EnableUserCodeException = true;
            })
            .ConfigureServices(services =>
            {
                services.AddDefaultServices();
                services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Constants.Environment.RedisConnectionString));

                //services.AddSingleton(typeof(IDistributedCacheService), typeof(DistributedCacheService));

                var mediatrAssemblies = assemblies.Where(a => a.GetTypes().Any(t => t.GetInterfaces().Any(i => i.Name.StartsWith("IRequestHandler`", StringComparison.OrdinalIgnoreCase))))
                                                                    .ToArray();

                var handlers = mediatrAssemblies.SelectMany(x => x.GetTypes())
                                                    .Where(x => x.GetInterfaces().Any(i => i.Name.StartsWith("IRequestHandler`", StringComparison.OrdinalIgnoreCase)))
                                                    .ToArray();

                foreach (var interfaceMap in handlers.Select(x => x.GetInterfaceMap(x.GetInterfaces().First(i => i.Name.StartsWith("IRequestHandler`", StringComparison.OrdinalIgnoreCase)))))
                    services.AddTransient(interfaceMap.InterfaceType, interfaceMap.TargetType);

                foreach (var invoke in configureServices)
                    invoke(services);

                services.AddMediatR(options => options.RegisterServicesFromAssemblies(mediatrAssemblies));

                var loadedServices = string.Empty;

                foreach (var service in services)
                    loadedServices += $"{service.ServiceType.FullName},{service.ImplementationType?.FullName}{System.Environment.NewLine}";

                System.Diagnostics.Debug.WriteLine(loadedServices);
            })
            .ConfigureOpenApi();
    }
}

