using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.WebSocketHost;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Tests
{
    [Collection("Native transport configuration")]
    public sealed class AppHostTransportOptionsTests
    {
        [Fact]
        public void NativeSubscriptionsRegisterWithoutEnableFlag()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            VAA.EAPI.GraphQL.FunctionHost.WebSockets.NativeWebSocketRegistration.AddNativeWebSocketSubscriptions(services);
            Assert.Contains(services, service => service.ServiceType == typeof(VAA.EAPI.GraphQL.FunctionHost.WebSockets.RedisSubscriptionSessionStore));
            Assert.Contains(services, service => service.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService));
        }

        [Theory]
        [InlineData("1", "graphql-dev", true)]
        [InlineData("2", "", false)]
        [InlineData("2", "graphql-dev", true)]
        public void BridgeRequiresRedisNamespaceWithMultipleWorkers(string workers, string prefix, bool valid)
        {
            var settings = new Dictionary<string, string?>
            {
                ["NativeWebSocketsRedisPrefix"] = prefix,
                ["FUNCTIONS_WORKER_PROCESS_COUNT"] = workers,
                ["NativeWebSocketsBridgeSecret"] = new string('x', 32),
                ["WEBSITE_INSTANCE_ID"] = null
            };
            var original = settings.Keys.ToDictionary(key => key, key => ConfigReader.Try(key));
            try
            {
                foreach (var setting in settings) { ConfigReader.Set(setting.Key, setting.Value); }
                if (valid) { Assert.Equal(new string('x', 32), TransportOptions.FromEnvironment().BridgeSecret); }
                else { Assert.Throws<InvalidOperationException>(() => TransportOptions.FromEnvironment()); }
            }
            finally
            {
                foreach (var setting in original) { ConfigReader.Set(setting.Key, setting.Value); }
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("localhost:7071")]
        [InlineData("app.azurewebsites.net")]
        public void InternalDispatchDoesNotRequirePlatformHostname(string? hostname)
        {
            var settings = new Dictionary<string, string?>
            {
                ["WEBSITE_INSTANCE_ID"] = null,
                ["WEBSITE_HOSTNAME"] = hostname,
                ["NativeWebSocketsRedisPrefix"] = "graphql-test",
                ["NativeWebSocketsBridgeSecret"] = new string('x', 32)
            };
            var original = settings.Keys.ToDictionary(key => key, key => ConfigReader.Try(key));
            try
            {
                foreach (var setting in settings) { ConfigReader.Set(setting.Key, setting.Value); }
                Assert.Equal(new string('x', 32), TransportOptions.FromEnvironment().BridgeSecret);
            }
            finally
            {
                foreach (var setting in original) { ConfigReader.Set(setting.Key, setting.Value); }
            }
        }

    }

    [CollectionDefinition("Native transport configuration", DisableParallelization = true)]
    public sealed class NativeTransportConfigurationCollection { }
}
