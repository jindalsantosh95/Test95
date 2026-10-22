using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Reference.Application.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Application.Query;
using VAA.EAPI.Microservices.Retail.Reference.Triggers.Http;
using Xunit;
using Environment = VAA.EAPI.Microservices.Retail.Reference.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Reference.Tests
{
    public sealed class MobileFeaturesQueryHandlerTests
    {
        [Theory]
        [InlineData("configured", 3)]
        [InlineData("empty", 0)]
        public async Task ReturnsConfiguredDictionary(string scenario, int count)
        {
            using var services = CreateServices(scenario);
            var handler = services.GetRequiredService<IMobileFeaturesQueryHandler>();
            var result = await handler.HandleAsync(CancellationToken.None);

            Assert.Equal(count, result.Features.Count);
            if (count > 0)
            {
                Assert.Equal("true", result.Features["nativeBooking"]);
                Assert.Equal("2.4", result.Features["minimumVersion"]);
                Assert.Equal("2026-10-12T00:00:00Z", result.Features["launchDate"]);
            }
            AssertBlobRequested(services, scenario);
        }

        [Theory]
        [InlineData("null-document")]
        [InlineData("missing-features")]
        [InlineData("null-features")]
        [InlineData("null-value")]
        [InlineData("blank-key")]
        [InlineData("invalid-json")]
        [InlineData("boolean-value")]
        [InlineData("integer-value")]
        [InlineData("decimal-value")]
        [InlineData("object-value")]
        [InlineData("array-value")]
        [InlineData("array-features")]
        public async Task RejectsInvalidConfiguration(string scenario)
        {
            using var services = CreateServices(scenario);
            var handler = services.GetRequiredService<IMobileFeaturesQueryHandler>();

            await Assert.ThrowsAnyAsync<JsonException>(() => handler.HandleAsync(CancellationToken.None));
            AssertBlobRequested(services, scenario);
        }

        [Theory]
        [InlineData("configured", 3)]
        [InlineData("empty", 0)]
        public async Task ControllerReturnsDictionaryThroughDirectInjection(string scenario, int count)
        {
            using var services = CreateServices(scenario);
            var controller = services.GetRequiredService<MobileFeaturesController>();
            using var body = new MemoryStream();
            var request = new Core.Functions.TestHttpRequestData(new Core.Functions.TestFunctionContext(),
                new Uri("https://tempuri.org/MobileFeatures"), body, "GET");

            var response = await controller.MobileFeatures(request, CancellationToken.None);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);
            var result = JObject.Parse(await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
            var features = Assert.IsType<JObject>(result["features"]);
            Assert.Equal(count, features.Count);
            if (count > 0)
            {
                Assert.Equal("true", (string?)features["nativeBooking"]);
            }
            AssertBlobRequested(services, scenario);
        }

        [Theory]
        [InlineData("not-found")]
        [InlineData("storage-failure")]
        public async Task StorageFailuresDoNotExposeConnectionDetails(string scenario)
        {
            using var services = CreateServices(scenario);
            var handler = services.GetRequiredService<IMobileFeaturesQueryHandler>();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(TestContext.Current.CancellationToken));

            Assert.Equal("Mobile feature configuration is unavailable.", exception.Message);
            Assert.Null(exception.InnerException);
            Assert.Empty(exception.Data);
            Assert.DoesNotContain("xxx", exception.ToString());
            Assert.DoesNotContain("tempuri.org", exception.ToString());
            AssertBlobRequested(services, scenario);
        }

        [Fact]
        public async Task PreservesCallerCancellation()
        {
            using var services = CreateServices("configured");
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var handler = services.GetRequiredService<IMobileFeaturesQueryHandler>();

            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.HandleAsync(cancellation.Token));

            Assert.Equal(cancellation.Token, exception.CancellationToken);
            Assert.Null(exception.InnerException);
        }

        private static ServiceProvider CreateServices(string scenario)
        {
            ConfigReader.Set(Environment.BlobStorageUri, "https://tempuri.org/");
            ConfigReader.Set(Environment.ContainerName, "container");
            ConfigReader.Set(Environment.SasKey, "xxx");
            ConfigReader.Set(Environment.MobileFeaturesBlob, $"MobileFeatures-{scenario}.json");

            return new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory, Core.Xunit.Http.HttpClientFactory>()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                .AddSingleton<IMobileFeaturesQueryHandler, MobileFeaturesQueryHandler>()
                .AddTransient<MobileFeaturesController>()
                .BuildServiceProvider();
        }

        private static void AssertBlobRequested(IServiceProvider services, string scenario)
        {
            var uri = $"https://tempuri.org/container/MobileFeatures-{scenario}.json?xxx";
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)services.GetRequiredService<IHttpClientFactory>();
            Assert.Contains(httpClientFactory.GetResponses(uri), response =>
                response.RequestMessage?.Method == HttpMethod.Get && response.RequestMessage.RequestUri?.ToString() == uri);
        }
    }
}
