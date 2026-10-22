using System;
using System.IO;
using System.Linq;
using System.Reflection;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;

namespace VAA.EAPI.GraphQL.FunctionHost
{
    public static class DomainStartupDiscovery
    {
        public static ConfigurationActions[] Discover(string directory, string filter = null) =>
            Directory.GetFiles(directory, "VAA.EAPI.GraphQL.Domain.*.dll", SearchOption.TopDirectoryOnly)
                .Distinct()
                .Select(path => Assembly.LoadFrom(Path.GetFullPath(path)))
                .Where(assembly => filter == null || filter.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Any(pattern => RegexCompiled.IsMatch(assembly.FullName, pattern.Trim())))
                .SelectMany(assembly => assembly.ExportedTypes
                    .Where(type => type.Name == "Startup" && typeof(IStartup).IsAssignableFrom(type))
                    .Take(1))
                .Select(type => (IStartup)Activator.CreateInstance(type))
                .Select(startup => new ConfigurationActions(
                    startup.ConfigureAppConfiguration,
                    startup.ConfigureFunctionsWorkerDefaults,
                    startup.ConfigureServices,
                    startup.RegisterQueries,
                    startup.RegisterMutations,
                    startup.RegisterHealthChecks,
                    startup.RegisterSubscriptions))
                .ToArray();
    }
}
