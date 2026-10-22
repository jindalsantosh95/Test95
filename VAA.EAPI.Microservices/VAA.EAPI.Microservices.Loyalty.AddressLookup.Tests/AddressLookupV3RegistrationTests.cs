using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Interface;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public sealed class AddressLookupV3RegistrationTests
    {
        [Fact]
        public void GbgRegistrations_ResolveV2AndV3ThroughConnectorAsSingletons()
        {
            ConfigReader.Set(Constants.Environment.GbgServiceKeyMap, "[{\"clientId\":\"\",\"gbgKey\":\"test-only-key\"}]");
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IHttpConnector, HttpConnector>();
            services.AddGbgServices();
            Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IGbgService));
            Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IGbgV3Service));
            using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

            var v2 = Assert.IsType<GbgService>(provider.GetRequiredService<IGbgService>());
            var v3 = Assert.IsType<GbgV3Service>(provider.GetRequiredService<IGbgV3Service>());

            Assert.Same(v2, provider.GetRequiredService<IGbgService>());
            Assert.Same(v3, provider.GetRequiredService<IGbgV3Service>());
        }
    }
}
