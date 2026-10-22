using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Services;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Core.Benchmark.Tests
{
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
    public class OfferApiBenchmark
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Microservices.Retail.Offer.Infrastructure.Proxy.result ITAResponse;
        private readonly OfferRequest offerRequest;
        private readonly string docId;
        private readonly string offerExpiryLimit;
        private readonly ICachedPriceClass cachedPriceClass;
        private readonly ICachedCabin cachedCabin;
        private readonly ICachedPieceAllowance cachedPieceAllowance;
        private readonly ICachedCabinBaggage cachedCabinBaggage;
        private readonly ICachedAirportDetails cachedAirportDetails;


        public OfferApiBenchmark()
        {
            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddMemoryCache()
                                                      .AddSingleton<IHttpClientFactory>(new HttpClientFactoryFixture().Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IAzureBlobService, Xunit.Azure.AzureBlobService>()
                                                      .AddSingleton<ICachedPriceClass, CachedPriceClass>()
                                                      .AddSingleton<ICachedCabin, CachedCabin>()
                                                      .AddSingleton<ICachedPieceAllowance, CachedPieceAllowance>()
                                                      .AddSingleton<ICachedCabinBaggage, CachedCabinBaggage>()
                                                      .AddSingleton<ICachedAirportDetails, CachedAirportDetails>()
                                                      .BuildServiceProvider();

            docId = Guid.NewGuid().ToString();
            offerExpiryLimit = "2025-11-26T15:59:16Z";

            cachedPriceClass = _serviceProvider.GetService<ICachedPriceClass>()!;
            cachedCabin = _serviceProvider.GetService<ICachedCabin>()!;
            cachedPieceAllowance = _serviceProvider.GetService<ICachedPieceAllowance>()!;
            cachedCabinBaggage = _serviceProvider.GetService<ICachedCabinBaggage>()!;
            cachedAirportDetails = _serviceProvider.GetService<ICachedAirportDetails>()!;

            ITAResponse = TestDataProvider.GetTestData<Microservices.Retail.Offer.Infrastructure.Proxy.result>("OfferMS\\ITAResponse");
            offerRequest = TestDataProvider.GetTestData<OfferRequest>("OfferMS\\OfferRequest");

        }

        [Benchmark]
        public void OfferMapper()
        {
            var response = OfferITAResponseMapper.OfferResponseMapper(ITAResponse, offerRequest, cachedPriceClass, cachedCabin, docId, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), cachedPieceAllowance, cachedCabinBaggage, cachedAirportDetails)
                                     .GetAwaiter()
                                     .GetResult();

        }
    }
}
