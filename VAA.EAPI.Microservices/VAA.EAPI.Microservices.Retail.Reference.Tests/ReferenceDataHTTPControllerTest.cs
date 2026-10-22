using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Reference.Application;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Triggers;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Reference.Tests
{
    public class ReferenceDataHTTPControllerTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public ReferenceDataHTTPControllerTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("BlobStorageUri", "https://tempuri.org/");
            ConfigReader.Set("ContainerName", "container");
            ConfigReader.Set("SasKey", "xxx");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                       .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                       .AddSingleton<IHttpConnector, HttpConnector>()
                                                       .AddSingleton<IAzureBlobService>(services =>
                                                           new Core.Xunit.Azure.AzureBlobService(services.GetRequiredService<IHttpConnector>()))
                                                      .AddSingleton<ReferenceDataCommandHandler>()
                                                      .AddMediatR(config =>
                                                          config.RegisterServicesFromAssembly(typeof(ReferenceDataCommandHandler).Assembly))
                                                      .AddSingleton<ReferenceDataController>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task FqtPrograms_ShouldSucceed()
        {
            var handler = _serviceProvider.GetService<ReferenceDataCommandHandler>()!;
            ConfigReader.Set("FQTProgramsBlob", "FQTProgrammes.json");

            ReferenceDataCommand cmd = new()
            {
                ReferenceDataType = ReferenceDataTypeEnum.FQTPROGS
            };

            var result = await handler.Handle(cmd, CancellationToken.None);
            var output = Json.Deserialize<IEnumerable<FQTProgram>>(result);

            Assert.Equal(25, output.Count());

        }



        [Fact]
        public async Task PaymentMethodDetails_ShouldSucceed()
        {
            var controller = _serviceProvider.GetService<ReferenceDataController>()!;
            ConfigReader.Set("PaymentMethodDetailsBlob", "PaymentMethodDetails.json");

            var bodyStream = new MemoryStream(Array.Empty<byte>());

            // Create a mock HttpRequestData
            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("https://tempuri.org"), bodyStream, "GET");
            var result = await controller.PaymentMethodDetails(request);

            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
            var sr = new StreamReader(result.Body);
            sr.BaseStream.Position = 0;
            var resultBody = sr.ReadToEnd().ToString();
            var output = Json.Deserialize<IEnumerable<PaymentMethod>>(resultBody);

            Assert.Equal(24, output.Count());
        }

        [Fact]
        public async Task AirlineDetails_ShouldReturnHomepageUrl()
        {
            ConfigReader.Set("AirlineBlob", "AirlineDetails.json");

            var controller = _serviceProvider.GetRequiredService<ReferenceDataController>();

            var request = new Core.Functions.TestHttpRequestData(
                new Core.Functions.TestFunctionContext(),
                new Uri("https://tempuri.org"),
                new MemoryStream(Array.Empty<byte>()),
                "GET");

            var result = await controller.AirlineDetails(request);

            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
            result.Body.Position = 0;
            using var reader = new StreamReader(result.Body);
            var output = Json.Deserialize<IEnumerable<AirlineDetails>>(await reader.ReadToEndAsync());
            var americanAirlines = Assert.Single(output);

            Assert.Equal("AA", americanAirlines.CarrierCode);
            Assert.Equal("American Airlines", americanAirlines.CarrierName);
            Assert.Equal("https://www.aa.com/homePage.do", americanAirlines.Url);
        }
    }
}
