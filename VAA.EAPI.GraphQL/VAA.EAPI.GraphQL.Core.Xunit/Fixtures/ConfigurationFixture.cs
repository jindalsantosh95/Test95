using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Core.Xunit.Fixtures
{
    public sealed class ConfigurationFixture : IDisposable
    {
        private static readonly object @lock = new();

        public static IConfigurationRoot? Current { get; private set; }

        public ConfigurationFixture()
        {
            if (Current == null)
                lock (@lock)
                {
                    Current ??= new ConfigurationBuilder()
                                .SetBasePath(Environment.CurrentDirectory)
                                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                                .AddJsonFile("dev.settings.json", optional: true, reloadOnChange: true)
                                .AddJsonFile("sit.settings.json", optional: true, reloadOnChange: true)
                                .AddJsonFile("uat.settings.json", optional: true, reloadOnChange: true)
                                .AddInMemoryCollection()
                                .Build();

                    var provider = Current.Providers.FirstOrDefault(x => x is JsonConfigurationProvider && x.GetChildKeys(Array.Empty<string>(), "Values").Any());

                    if (provider != null)
                        foreach (var key in provider.GetChildKeys(Array.Empty<string>(), "Values"))
                            if (provider.TryGet($"Values:{key}", out string? value))
                                ConfigReader.Set(key, value);
                }
        }

        public void InitialiseValueFromKeyVault(string key, string secretName, KeyVault vault) => InitialiseValue(key, vault.GetSecretValue(secretName));
        public void InitialiseValue(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (value is null) return;

            var jsonProvider = Current!.Providers.FirstOrDefault(x => x is JsonConfigurationProvider && x.GetChildKeys(Array.Empty<string>(), "Values").Any());

            string? currentValue = null;

            jsonProvider?.TryGet($"Values:{key}", out currentValue);

            if (currentValue == value) return;

            var provider = Current!.Providers.First(x => x is MemoryConfigurationProvider)!;

            if (!provider.TryGet($"Values:{key}", out _))
                lock (@lock)
                    if (!provider.TryGet($"Values:{key}", out _))
                    {
                        provider.Set($"Values:{key}", value);

                        ConfigReader.Set(key, value);
                    }
        }

        public void Dispose()
        {
        }
    }
}
