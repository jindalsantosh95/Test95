using Microsoft.Extensions.DependencyInjection;
using System.Xml.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Reference.Application.Command;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Reference.Mapper;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Reference.Tests
{
    public class CurrencyConversionHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly IServiceProvider _serviceProvider;

        public CurrencyConversionHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _httpClientFactoryFixture = httpClientFactoryFixture;

            ConfigReader.Set("BlobStorageUri", "https://tempuri.org/");
            ConfigReader.Set("ContainerName", "container");
            ConfigReader.Set("SasKey", "xxx");
            ConfigReader.Set("CurrencyConversionSubKey", "asdf");
            ConfigReader.Set("CurrencyConversionServiceUrl", "https://tempuri.org/Currency");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                                                      .AddSingleton<CurrencyConversionCommandHandler>()
                                                      .AddScoped<ICurrencyConversionService, CurrencyConversionService>()
                                                      .BuildServiceProvider();

            var storageAccountUrl = ConfigReader.Get("BlobStorageUri");
            var container = ConfigReader.Get("ContainerName");
            var name = "CurrencyConversion.json";
            var sasKey = ConfigReader.Get("SasKey");
            var content = "{\"usdToGbpExchangeRate\":0.73,\"updateTimestamp\":\"2026-02-09T11:51:32\"}";

            var azureBlobService = _serviceProvider.GetService<IAzureBlobService>()!;

            azureBlobService.UploadBlobAsync(storageAccountUrl, container, sasKey, name, content, BlobType.Json, true, CancellationToken.None)
                            .Wait();
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            var handler = _serviceProvider.GetService<CurrencyConversionCommandHandler>()!;

            await handler.Handle();

            // If no exceptions are thrown, the test is considered successful.

            var responses = _httpClientFactoryFixture.Current.GetResponses("https://tempuri.org/Currency");

            Assert.True(responses[responses.Count - 1].IsSuccessStatusCode);
        }

        [Fact]
        public void RequestMapper_Test()
        {
            var xmlRequest = CurrencyConversionRequestMapper.MapCurrencyConversionRequestToXml();
            
            Assert.NotNull(xmlRequest);
            
            Assert.Contains("originalCurrencyISOCode=\"USD\"", xmlRequest);
            Assert.Contains("targetCurrencyISOCode=\"GBP\"", xmlRequest);
            Assert.Contains("ConvertCurrency", xmlRequest);
        }

        [Fact]
        public async Task CurrencyConversionService_Test()
        {
            var service = _serviceProvider.GetService<ICurrencyConversionService>()!;

            var request = CurrencyConversionRequestMapper.MapCurrencyConversionRequestToXml();

            var response = await service.Handle(request);

            var exchangeRate = XDocument.Parse(response)
                                        .Descendants(XName.Get("Response", "http://schemas.virgin-atlantic.com/FinancialServices/ExchangeRateRequest/2007/12"))
                                        .FirstOrDefault()?
                                        .Attribute("convertedValue")?
                                        .Value;

            Assert.NotNull(response);
            Assert.NotNull(exchangeRate);
            Assert.Equal("0.7313", exchangeRate);
        }

        [Fact]
        public void DecimalRoundOff_Test()
        {
            decimal num1 = 0.1234M;
            decimal num2 = 0.73135M;
            decimal num3 = 1.9999M;

            var result = RoundOffHelper.RoundTo2(num1);
            var result2 = RoundOffHelper.RoundTo2(num2);
            var result3 = RoundOffHelper.RoundTo2(num3);

            Assert.Equal(0.12M, result);
            Assert.Equal(0.73M, result2);
            Assert.Equal(2.00M, result3);
        }

    }
}
